using System.Collections.Concurrent;
using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;
using Plus.HabboHotel.Cache.Process;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Cache;

public class CacheManager : ICacheManager
{
    private readonly ILogger<CacheManager> _logger;
    private readonly IProcessComponent _process;
    private readonly IDatabase _database;
    private readonly IGameClientManager _gameClientManager;
    private readonly ConcurrentDictionary<int, CachedUser> _usersCached;

    public CacheManager(IProcessComponent processComponent, IDatabase database, IGameClientManager gameClientManager, ILogger<CacheManager> logger)
    {
        _process = processComponent;
        _database = database;
        _gameClientManager = gameClientManager;
        _logger = logger;
        _usersCached = new();
    }

    public void Init()
    {
        _process.Init();
        _logger.LogInformation("Cache Manager -> LOADED");
    }

    public bool ContainsUser(int id) => _usersCached.ContainsKey(id);

    public CachedUser? GenerateUser(int id)
    {
        if (TryGetUser(id, out var cachedUser))
        {
            cachedUser.AddedTime = DateTime.UtcNow;
            return cachedUser;
        }

        var client = _gameClientManager.GetClientByUserId(id);
        if (client?.GetHabbo() != null)
        {
            cachedUser = new() { Id = id, Username = client.GetHabbo().Username, Motto = client.GetHabbo().Motto, Look = client.GetHabbo().Look};
            _usersCached.TryAdd(id, cachedUser);
            return cachedUser;
        }

        using var connection = _database.Connection();
        cachedUser = connection.QuerySingleOrDefaultAsync<CachedUser>("SELECT id, `username`, `motto`, `look` FROM users WHERE id = @id LIMIT 1", new { id }).Result;
        if (cachedUser != null)
            _usersCached.TryAdd(id, cachedUser);
        return cachedUser;
    }

    public bool TryRemoveUser(int id, out CachedUser cachedUser) => _usersCached.TryRemove(id, out cachedUser);

    public bool TryGetUser(int id, out CachedUser cachedUser) => _usersCached.TryGetValue(id, out cachedUser);

    public ICollection<CachedUser> GetUserCache() => _usersCached.Values;
}