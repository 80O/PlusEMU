using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;

namespace Plus.Core.Settings;

public class SettingsManager : ISettingsManager
{
    private readonly IDatabase _database;
    private readonly ILogger<SettingsManager> _logger;
    private Dictionary<string, string> _settings = new(0);

    public SettingsManager(IDatabase database, ILogger<SettingsManager> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task Reload()
    {
        using var connection = _database.Connection();
        _settings = (await connection.QueryAsync<(string, string)>("SELECT `key`, `value` FROM `server_settings`")).ToDictionary(x => x.Item1, x => x.Item2.ToLower());
        _logger.LogInformation("Loaded " + _settings.Count + " server settings.");
    }

    public string TryGetValue(string value) => _settings.ContainsKey(value) ? _settings[value] : "0";
}