using Plus.HabboHotel.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Profile
{
    public interface IProfileManager
    {
        Habbo? GetProfile(Habbo habbo);
        int GetFriendCount(Habbo habbbo);
        List<Group> GetGroups(Habbo habbo);
    }
}
