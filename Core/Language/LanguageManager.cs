using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;

namespace Plus.Core.Language;

public class LanguageManager : ILanguageManager
{
    private readonly IDatabase _database;
    private readonly ILogger<LanguageManager> _logger;
    private Dictionary<string, string> _values = new(0);

    public LanguageManager(IDatabase database, ILogger<LanguageManager> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task Reload()
    {
        using var connection = _database.Connection();
        _values = (await connection.QueryAsync<(string, string)>("SELECT `key`, `value` FROM `server_locale`")).ToDictionary(x => x.Item1, x => x.Item2);
        _logger.LogInformation("Loaded " + _values.Count + " language locales.");
    }

    public string TryGetValue(string value) => _values.ContainsKey(value) ? _values[value] : "No language locale found for [" + value + "]";
}