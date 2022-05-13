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

        public async Task<int> GetFriendCount (int userid)
        {
            using var connection = _database.Connection();
            return await connection.ExecuteScalarAsync<int>("SELECT count(0) FROM messenger_friendships WHERE user_one_id = @userId OR user_two_id = @userId", new { userId = userid });
        }
    }
}
