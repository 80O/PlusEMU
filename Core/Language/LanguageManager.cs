using Dapper;
using NLog;
using Plus.Database;

namespace Plus.Core.Language;

public class LanguageManager : ILanguageManager
{
    private readonly IDatabase _database;
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Core.Language.LanguageManager");
    private Dictionary<string, string> _values = new(0);

    public LanguageManager(IDatabase database)
    {
        _database = database;
    }

    public async Task Reload()
    {
        using var connection = _database.Connection();
        _values = (await connection.QueryAsync<(string, string)>("SELECT `key`, `value` FROM `server_locale`")).ToDictionary(x => x.Item1, x => x.Item2);
        Log.Info("Loaded " + _values.Count + " language locales.");
    }

    public string TryGetValue(string value) => _values.ContainsKey(value) ? _values[value] : "No language locale found for [" + value + "]";
}