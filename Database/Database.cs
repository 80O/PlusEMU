using MySqlConnector;
using Plus.Core;
using Plus.Database.Interfaces;
using System.Data;

namespace Plus.Database;

public sealed class Database : IDatabase
{
    private readonly string _connectionStr;

    public Database(ConfigurationData configurationData)
    {
        _connectionStr = new MySqlConnectionStringBuilder
        {
            ConnectionTimeout = 10,
            Database = configurationData.Data["db.name"],
            DefaultCommandTimeout = 30,
            MaximumPoolSize = uint.Parse(configurationData.Data["db.pool.maxsize"]),
            MinimumPoolSize = uint.Parse(configurationData.Data["db.pool.minsize"]),
            Password = configurationData.Data["db.password"],
            Pooling = true,
            Port = uint.Parse(configurationData.Data["db.port"]),
            Server = configurationData.Data["db.hostname"],
            UserID = configurationData.Data["db.username"],
            AllowZeroDateTime = true,
            ConvertZeroDateTime = true,
            SslMode = MySqlSslMode.None
        }.ToString();
    }

    public bool IsConnected()
    {
        try
        {
            var con = new MySqlConnection(_connectionStr);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT 1+1";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            con.Close();
        }
        catch (MySqlException)
        {
            return false;
        }
        return true;
    }

    public IQueryAdapter GetQueryReactor()
    {
        try
        {
            IDatabaseClient dbConnection = new DatabaseConnection(_connectionStr);
            dbConnection.Connect();
            return dbConnection.GetQueryReactor();
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
            return null;
        }
    }

    public IDbConnection Connection() => new MySqlConnection(_connectionStr);
}