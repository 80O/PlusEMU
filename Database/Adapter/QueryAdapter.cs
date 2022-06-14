using MySqlConnector;
using Plus.Core;
using Plus.Database.Interfaces;
using System.Data;

namespace Plus.Database.Adapter;

public class QueryAdapter : IRegularQueryAdapter
{
    protected IDatabaseClient Client;
    protected MySqlCommand Command;

    public bool DbEnabled = true;

    public QueryAdapter(IDatabaseClient client)
    {
        Client = client;
    }

    public void AddParameter(string parameterName, object val)
    {
        Command.Parameters.AddWithValue(parameterName, val);
    }

    public bool FindsResult()
    {
        var hasRows = false;
        try
        {
            using var reader = Command.ExecuteReader();
            hasRows = reader.HasRows;
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return hasRows;
    }

    public int GetInteger()
    {
        var result = 0;
        try
        {
            var obj2 = Command.ExecuteScalar();
            if (obj2 != null) int.TryParse(obj2.ToString(), out result);
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return result;
    }

    public DataRow GetRow
        ()
    {
        DataRow row = null;
        try
        {
            var dataSet = new DataSet();
            using (var adapter = new MySqlDataAdapter(Command))
            {
                adapter.Fill(dataSet);
            }
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count == 1) row = dataSet.Tables[0].Rows[0];
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return row;
    }

    public string GetString()
    {
        var str = string.Empty;
        try
        {
            var obj2 = Command.ExecuteScalar();
            if (obj2 != null) str = obj2.ToString();
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return str;
    }

    public DataTable GetTable()
    {
        var dataTable = new DataTable();
        if (!DbEnabled)
            return dataTable;
        try
        {
            using var adapter = new MySqlDataAdapter(Command);
            adapter.Fill(dataTable);
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return dataTable;
    }

    public void RunQuery(string query)
    {
        if (!DbEnabled)
            return;
        SetQuery(query);
        RunQuery();
    }

    public void SetQuery(string query)
    {
        Command.Parameters.Clear();
        Command.CommandText = query;
    }

    public long InsertQuery()
    {
        if (!DbEnabled)
            return 0;
        var lastInsertedId = 0L;
        try
        {
            Command.ExecuteScalar();
            lastInsertedId = Command.LastInsertedId;
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
        return lastInsertedId;
    }

    public void RunQuery()
    {
        if (!DbEnabled)
            return;
        try
        {
            Command.ExecuteNonQuery();
        }
        catch (Exception exception)
        {
            ExceptionLogger.LogQueryError(Command.CommandText, exception);
        }
    }
}