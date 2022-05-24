using Dapper;
using Plus.Database;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Wardrobe
{
	public class UserWardrobeManager : IUserWardrobeManager
	{

        private readonly IDatabase _database;

        public UserWardrobeManager(IDatabase database)
        {
            _database = database;
        }

        public async Task SaveOutfit(int userID, int slotID, string gender, string look)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync(
                "REPLACE INTO `user_wardrobe` SET `look` = @look, `gender` = @gender WHERE `user_id` = @userID AND `slot_id` = @slotID LIMIT 1",
                    new {
                        look = look,
                        gender = gender.ToUpper(),
                        userID = userID,
                        lotID = slotID
                    }
            );

        }

		public async Task RemoveOutfit(int userID, int slotID, string gender)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync(
               "DELETE FROM `user_wardrobe` WHERE `user_id` = @userID AND `slot_id` = @slotID AND `gender` = @gender",
                   new
                   {
                       gender = gender.ToUpper(),
                       userID = userID,
                       lotID = slotID
                   }
           );
        }



	}
}
