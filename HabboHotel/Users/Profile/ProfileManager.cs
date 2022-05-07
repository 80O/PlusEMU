using Dapper;
using Plus.Database;
using Plus.HabboHotel.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Profile
{
    public class ProfileManager : IProfileManager
    {
        private readonly IGroupManager _groupManager;
        private readonly IDatabase _database;
        int friendCount;

        public ProfileManager(IGroupManager groupManager, IDatabase database)
        {
            _groupManager = groupManager;
            _database = database;
        }

        public List<Group> GetGroups(Habbo habbo)
        {
            var groups = _groupManager.GetGroupsForUser(habbo.Id);
            return groups;
        }

        public Habbo? GetProfile(Habbo habbo)
        {
            var targetData = PlusEnvironment.GetHabboById(habbo.Id);
            if (targetData == null)
            {
                habbo.GetClient().SendNotification("An error occured whilst finding that user's profile.");
                return null;
            }
            var groups = _groupManager.GetGroupsForUser(targetData.Id);
            
            return habbo;
        }

        public int GetFriendCount (Habbo habbo)
        {
            using (var connection = _database.Connection())
            {
                friendCount = connection.ExecuteScalar<int>("SELECT count(0) FROM messenger_friendships WHERE user_one_id = @userid OR user_two_id = @userid", new { userid = habbo.Id });
            }
            return friendCount;
        }
    }
}
