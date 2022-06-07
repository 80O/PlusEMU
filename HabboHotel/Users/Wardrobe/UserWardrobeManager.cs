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
                "INSERT INTO `user_wardrobe` (`user_id`, `slot_id`, `gender`, `look`) VALUES (@userID, @slotID, @gender, @look) ON DUPLICATE KEY UPDATE `look`=VALUES(`look`)",
                    new {
                        userID = userID,
                        slotID = slotID,
                        gender = gender.ToUpper(),
                        look = look,
                    }
            );

        }

		public async Task RemoveOutfit(int userID, int slotID)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync(
               "DELETE FROM `user_wardrobe` WHERE `user_id` = @userID AND `slot_id` = @slotID",
                   new
                   {
                       userID = userID,
                       slotID = slotID
                   }
           );
        }



	}
}
