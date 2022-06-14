using Plus.Database.Interfaces;
using System.Data;

namespace Plus.Database;

public interface IDatabase
{
    bool IsConnected();
    [Obsolete]
    IQueryAdapter GetQueryReactor();
    IDbConnection Connection();
}