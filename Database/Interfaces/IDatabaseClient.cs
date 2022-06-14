using MySqlConnector;

namespace Plus.Database.Interfaces;

public interface IDatabaseClient : IDisposable
{
    void Connect();
    void Disconnect();
    IQueryAdapter GetQueryReactor();
    MySqlCommand CreateNewCommand();
}