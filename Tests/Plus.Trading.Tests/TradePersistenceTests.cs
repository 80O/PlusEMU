using Dapper;
using MySqlConnector;
using Plus.HabboHotel.Rooms.Trading;
using Xunit;

namespace Plus.Trading.Tests;

public sealed class MySqlFactAttribute : FactAttribute
{
    public MySqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PLUS_TRADE_TEST_MYSQL")))
            Skip = "Set PLUS_TRADE_TEST_MYSQL to an isolated MySQL/MariaDB server with CREATE DATABASE permission.";
    }
}

public sealed class TradePersistenceTests : IDisposable
{
    private readonly string? _server = Environment.GetEnvironmentVariable("PLUS_TRADE_TEST_MYSQL");
    private readonly string _database = "plus_trade_test_" + Guid.NewGuid().ToString("N");
    private readonly string? _connectionString;

    public TradePersistenceTests()
    {
        if (string.IsNullOrWhiteSpace(_server)) return;
        using var server = new MySqlConnection(_server);
        server.Open();
        server.Execute($"CREATE DATABASE `{_database}`");
        _connectionString = new MySqlConnectionStringBuilder(_server) { Database = _database }.ConnectionString;
        using var connection = Open();
        connection.Execute("CREATE TABLE users (id INT PRIMARY KEY, credits INT NOT NULL) ENGINE=InnoDB; " +
            "CREATE TABLE items (id INT UNSIGNED PRIMARY KEY, user_id INT NOT NULL, room_id INT NOT NULL DEFAULT 0) ENGINE=InnoDB; " +
            "CREATE TABLE logs_client_trade (id INT PRIMARY KEY AUTO_INCREMENT, user_one INT, user_two INT, items_one TEXT, items_two TEXT, traded_at BIGINT) ENGINE=InnoDB; " +
            "INSERT INTO users VALUES (1, 100), (2, 200); INSERT INTO items VALUES (10, 1, 0), (20, 2, 0)");
    }

    private MySqlConnection Open()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private void Commit(TradeItemTransfer[]? items = null, TradeCreditBalance[]? balances = null)
    {
        using var connection = new MySqlConnection(_connectionString);
        TradePersistence.Commit(connection, 1, 2, items ?? new[] { new TradeItemTransfer(10, 1, 2, false), new TradeItemTransfer(20, 2, 1, false) },
            balances ?? Array.Empty<TradeCreditBalance>());
    }

    private void AssertOriginalState()
    {
        using var connection = Open();
        Assert.Equal(1, connection.ExecuteScalar<int>("SELECT user_id FROM items WHERE id=10"));
        Assert.Equal(2, connection.ExecuteScalar<int>("SELECT user_id FROM items WHERE id=20"));
        Assert.Equal(100, connection.ExecuteScalar<int>("SELECT credits FROM users WHERE id=1"));
        Assert.Equal(200, connection.ExecuteScalar<int>("SELECT credits FROM users WHERE id=2"));
        Assert.Equal(0, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM logs_client_trade"));
    }

    [MySqlFact]
    public void SuccessfulTradeCommitsBothDirectionsAndAuditLog()
    {
        Commit();
        using var connection = Open();
        Assert.Equal(2, connection.ExecuteScalar<int>("SELECT user_id FROM items WHERE id=10"));
        Assert.Equal(1, connection.ExecuteScalar<int>("SELECT user_id FROM items WHERE id=20"));
        Assert.Equal("10;", connection.ExecuteScalar<string>("SELECT items_one FROM logs_client_trade"));
        Assert.Equal("20;", connection.ExecuteScalar<string>("SELECT items_two FROM logs_client_trade"));
        Assert.Equal(1, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM logs_client_trade"));
    }

    [MySqlFact]
    public void FailureOnSecondItemRollsBackFirstItem()
    {
        Assert.Throws<InvalidOperationException>(() => Commit(new[] { new TradeItemTransfer(10, 1, 2, false), new TradeItemTransfer(20, 1, 2, false) }));
        AssertOriginalState();
    }

    [MySqlFact]
    public void PlacedItemCannotBeTraded()
    {
        using (var connection = Open()) connection.Execute("UPDATE items SET room_id=123 WHERE id=20");
        Assert.Throws<InvalidOperationException>(() => Commit());
        AssertOriginalState();
    }

    [MySqlFact]
    public void RedemptionPersistsCreditsAndDeletesRedeemableInSameTransaction()
    {
        Commit(new[] { new TradeItemTransfer(10, 1, 2, true), new TradeItemTransfer(20, 2, 1, false) },
            new[] { new TradeCreditBalance(2, 250) });
        using var connection = Open();
        Assert.Equal(0, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM items WHERE id=10"));
        Assert.Equal(250, connection.ExecuteScalar<int>("SELECT credits FROM users WHERE id=2"));
        Assert.Equal(1, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM logs_client_trade"));
    }

    [MySqlFact]
    public void AuditFailureRollsBackTransfersRedemptionAndCredits()
    {
        using (var connection = Open()) connection.Execute("CREATE TRIGGER fail_trade_log BEFORE INSERT ON logs_client_trade FOR EACH ROW SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Injected audit failure'");
        Assert.Throws<MySqlException>(() => Commit(new[] { new TradeItemTransfer(10, 1, 2, true), new TradeItemTransfer(20, 2, 1, false) },
            new[] { new TradeCreditBalance(2, 250) }));
        AssertOriginalState();
    }

    [MySqlFact]
    public void CreditWriteFailureRollsBackItemTransfers()
    {
        using (var connection = Open()) connection.Execute("CREATE TRIGGER fail_trade_credits BEFORE UPDATE ON users FOR EACH ROW SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Injected credit failure'");
        Assert.Throws<MySqlException>(() => Commit(balances: new[] { new TradeCreditBalance(2, 250) }));
        AssertOriginalState();
    }

    [MySqlFact]
    public void NonTransactionalTableIsRejectedBeforeWrites()
    {
        using (var connection = Open()) connection.Execute("ALTER TABLE logs_client_trade ENGINE=MyISAM");
        Assert.Throws<InvalidOperationException>(() => Commit());
        AssertOriginalState();
    }

    [MySqlFact]
    public void DuplicateItemIsRejectedWithoutChanges()
    {
        Assert.Throws<InvalidOperationException>(() => Commit(new[] { new TradeItemTransfer(10, 1, 2, false), new TradeItemTransfer(10, 1, 2, false) }));
        AssertOriginalState();
    }

    [MySqlFact]
    public void RetryingPersistedTradeCannotTransferAgainOrCreateSecondLog()
    {
        Commit();
        Assert.Throws<InvalidOperationException>(() => Commit());
        using var connection = Open();
        Assert.Equal(1, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM logs_client_trade"));
        Assert.Equal(2, connection.ExecuteScalar<int>("SELECT user_id FROM items WHERE id=10"));
    }

    public void Dispose()
    {
        if (string.IsNullOrWhiteSpace(_server)) return;
        using var server = new MySqlConnection(_server);
        server.Open();
        server.Execute($"DROP DATABASE IF EXISTS `{_database}`");
    }
}
