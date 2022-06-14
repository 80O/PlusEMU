using Dapper;
using Plus.Database;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.Users.Ignores
{
    internal class IgnoresUserDataLoadingTask : IUserDataLoadingTask
    {
        private readonly IDatabase _database;

        public IgnoresUserDataLoadingTask(IDatabase database)
        {
            _database = database;
        }

        public async Task Load(Habbo habbo)
        {
            using var connection = _database.Connection();
            var ignoredUsers = (await connection.QueryAsync<int>("SELECT * FROM `user_ignores` WHERE `user_id` = @userId", new { userId = habbo.Id })).ToList();
            habbo.IgnoresComponent = new(ignoredUsers);
        }
    }
}
