using MySqlConnector;
using Plus.Database.Adapter;
using Plus.Database.Interfaces;
using System.Data;

namespace Plus.Database;

public class DatabaseConnection : IDatabaseClient
{
    private readonly IQueryAdapter _adapter;
    private readonly MySqlConnection _con;

    public DatabaseConnection(string connectionString)
    {
        _con = new MySqlConnection(connectionString);
        _adapter = new NormalQueryReactor(this);
    }

    public void Connect()
    {
        if (_con.State == ConnectionState.Closed)
        {
            try
            {
                _con.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public void Disconnect()
    {
        if (_con.State == ConnectionState.Open) _con.Close();
    }

    public IQueryAdapter GetQueryReactor() => _adapter;

    public MySqlCommand CreateNewCommand() => _con.CreateCommand();

    public void Dispose()
    {
        if (_con.State == ConnectionState.Open) _con.Close();
        _con.Dispose();
        GC.SuppressFinalize(this);
    }
}