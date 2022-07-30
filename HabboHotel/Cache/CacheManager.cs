using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Plus.HabboHotel.Cache.Process;
using Plus.HabboHotel.Cache.Type;

namespace Plus.HabboHotel.Cache;

public class CacheManager : ICacheManager
{
    private readonly ILogger<CacheManager> _logger;
    private readonly IProcessComponent _process;
    private readonly ConcurrentDictionary<int, UserCache> _usersCached;

    public CacheManager(IProcessComponent processComponent, ILogger<CacheManager> logger)
    {
        _usersCached = new ConcurrentDictionary<int, UserCache>();
        _process = processComponent;
        _logger = logger;
    }

    public void Init()
    {

        _process.Init();
        _logger.LogInformation("Cache Manager -> LOADED");
    }

    public bool ContainsUser(int id) => _usersCached.ContainsKey(id);

    public UserCache GenerateUser(int id)
    {
        UserCache user = null;
        if (_usersCached.ContainsKey(id))
        {
            if (TryGetUser(id, out user))
                return user;
        }
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(id);
        if (client != null)
        {
            if (client.GetHabbo() != null)
            {
                user = new UserCache(id, client.GetHabbo().Username, client.GetHabbo().Motto, client.GetHabbo().Look);
                _usersCached.TryAdd(id, user);
                return user;
            }
        }
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `username`, `motto`, `look` FROM users WHERE id = @id LIMIT 1");
        dbClient.AddParameter("id", id);
        var dRow = dbClient.GetRow();
        if (dRow != null)
        {
            user = new UserCache(id, dRow["username"].ToString(), dRow["motto"].ToString(), dRow["look"].ToString());
            _usersCached.TryAdd(id, user);
        }
        return user;
    }

    public bool TryRemoveUser(int id, out UserCache user) => _usersCached.TryRemove(id, out user);

    public bool TryGetUser(int id, out UserCache user) => _usersCached.TryGetValue(id, out user);

    public ICollection<UserCache> GetUserCache() => _usersCached.Values;
}