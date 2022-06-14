using Dapper;
using Plus.Database;

namespace Plus.HabboHotel.Users.Ignores
{
    public interface IIgnoredUsersService
    {
        Task<List<string>> GetIgnoredUsersByName(IEnumerable<int> userIds);
    }
    internal class IgnoredUsersService : IIgnoredUsersService
    {
        private readonly IDatabase _database;

        public IgnoredUsersService(IDatabase database)
        {
            _database = database;
        }

        public async Task<List<string>> GetIgnoredUsersByName(IEnumerable<int> userIds)
        {
            using var connection = _database.Connection();
            return (await connection.QueryAsync<string>("SELECT username FROM users WHERE id in @userIds", new { userIds })).ToList();
        }
    }
}
