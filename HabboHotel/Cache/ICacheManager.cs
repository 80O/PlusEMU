using Plus.HabboHotel.Cache.Type;

namespace Plus.HabboHotel.Cache;

public interface ICacheManager
{
    bool ContainsUser(int id);
    CachedUser? GenerateUser(int id);
    bool TryRemoveUser(int id, out CachedUser cachedUser);
    bool TryGetUser(int id, out CachedUser cachedUser);
    ICollection<CachedUser> GetUserCache();
    void Init();
}