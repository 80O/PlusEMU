using Plus.HabboHotel.Cache.Type;

namespace Plus.HabboHotel.Cache;

public interface ICacheManager
{
    bool ContainsUser(int id);
    UserCache GenerateUser(int id);
    bool TryRemoveUser(int id, out UserCache user);
    bool TryGetUser(int id, out UserCache user);
    ICollection<UserCache> GetUserCache();
    void Init();
}