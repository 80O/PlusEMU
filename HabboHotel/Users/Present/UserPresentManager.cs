using System;
using Dapper;
using Plus.Database;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Present
{
	public class UserPresentManager : IUserPresentManager
	{

		private readonly IDatabase _database;


		public UserPresentManager(IDatabase database)
		{
			_database = database;
		}

		public async Task<int> CreatePresent(int itemID, int baseItemID, string itemExtraData)
        {
			using var connection = _database.Connection();
			var insertQuery = await connection.ExecuteAsync(
				"INSERT INTO `user_presents` (`item_id`,`base_id`,`extra_data`) VALUES (@itemID, @baseItemID, @itemExtraData)",
				new { itemID = itemID, baseItemID = baseItemID, itemExtraData = itemExtraData }
			);
			return Convert.ToInt32(insertQuery);
		}
	}
}