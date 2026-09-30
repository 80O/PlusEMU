using System.Data;
using Dapper;

namespace Plus.HabboHotel.Rooms.Trading;

public sealed record TradeItemTransfer(uint ItemId, int FromUserId, int ToUserId, bool Redeem);

public sealed record TradeCreditBalance(int UserId, int Credits);

/// <summary>Persists a trade before any inventory changes or client notifications.</summary>
public static class TradePersistence
{
    public static void Commit(IDbConnection connection, int userOneId, int userTwoId,
        IReadOnlyCollection<TradeItemTransfer> items, IReadOnlyCollection<TradeCreditBalance> balances)
    {
        if (userOneId == userTwoId || items.Select(i => i.ItemId).Distinct().Count() != items.Count)
            throw new InvalidOperationException("Invalid trade participants or duplicate items.");
        if (items.Any(i => !((i.FromUserId == userOneId && i.ToUserId == userTwoId) ||
                             (i.FromUserId == userTwoId && i.ToUserId == userOneId))) ||
            balances.Any(b => b.UserId != userOneId && b.UserId != userTwoId) ||
            balances.Select(b => b.UserId).Distinct().Count() != balances.Count)
            throw new InvalidOperationException("Invalid trade transfer or credit recipient.");

        connection.Open();
        // Old hotel databases may use MyISAM, which silently ignores rollback.
        var transactionalTables = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() " +
            "AND table_name IN ('items', 'users', 'logs_client_trade') AND engine = 'InnoDB'");
        if (transactionalTables != 3)
            throw new InvalidOperationException("Trading requires InnoDB tables: items, users, logs_client_trade.");

        using var transaction = connection.BeginTransaction();
        foreach (var item in items.OrderBy(i => i.ItemId))
        {
            var sql = item.Redeem
                ? "DELETE FROM items WHERE id = @ItemId AND user_id = @FromUserId AND room_id = 0"
                : "UPDATE items SET user_id = @ToUserId WHERE id = @ItemId AND user_id = @FromUserId AND room_id = 0";
            if (connection.Execute(sql, item, transaction) != 1)
                throw new InvalidOperationException($"Trade item {item.ItemId} is no longer in its owner's inventory.");
        }

        foreach (var balance in balances.OrderBy(b => b.UserId))
        {
            if (connection.Execute("UPDATE users SET credits = @Credits WHERE id = @UserId", balance, transaction) != 1)
                throw new InvalidOperationException("Trade credit recipient no longer exists.");
        }

        connection.Execute(
            "INSERT INTO logs_client_trade VALUES(null, @UserOneId, @UserTwoId, @UserOneItems, @UserTwoItems, UNIX_TIMESTAMP())",
            new
            {
                UserOneId = userOneId,
                UserTwoId = userTwoId,
                UserOneItems = string.Concat(items.Where(i => i.FromUserId == userOneId).Select(i => $"{i.ItemId};")),
                UserTwoItems = string.Concat(items.Where(i => i.FromUserId == userTwoId).Select(i => $"{i.ItemId};"))
            }, transaction);
        try
        {
            transaction.Commit();
        }
        catch (Exception exception)
        {
            throw new TradeCommitOutcomeUnknownException(exception);
        }
    }
}

public sealed class TradeCommitOutcomeUnknownException : Exception
{
    public TradeCommitOutcomeUnknownException(Exception innerException)
        : base("The database did not acknowledge the trade commit; reload persisted balances.", innerException)
    {
    }
}
