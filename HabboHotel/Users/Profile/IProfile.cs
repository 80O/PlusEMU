using Plus.HabboHotel.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Profile
{
    public interface IProfile
    {
        Task<Habbo>? GetProfile(Habbo habbo);
        Task<int> GetFriendCount(Habbo habbbo);
        Task<List<Group>> GetGroups(Habbo habbo);
    }
}
