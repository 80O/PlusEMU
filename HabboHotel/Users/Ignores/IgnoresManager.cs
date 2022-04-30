using Dapper;
using Plus.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Ignores
{
    public class IgnoresManager : IIgnoresManager
    {
        private readonly IDatabase _database;
        public IgnoresManager(IDatabase database)
        {
            _database = database;
        }

        public async Task UnIgnoreUser(int uid, int ignoreid)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync(
                "DELETE FROM user_ignores WHERE user_id = @uid AND ignore_id = @ignoreId",
                new { uid = uid, ignoreId = ignoreid }
                );
        }
    }
}
