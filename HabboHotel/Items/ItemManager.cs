using System;
using Dapper;
using Plus.Database;
using System.Threading.Tasks;
namespace Plus.HabboHotel.Items
{
	public class ItemManager : IItemManager
	{

		private readonly IDatabase _database;


		public ItemManager(IDatabase database)
		{
			_database = database;
		}

		public async Task<int> CreateItem(int userID, int baseItemID, string extraData)
        {
			using var connection = _database.Connection();
			var InsertQuery = await connection.ExecuteAsync(
				"INSERT INTO `items` (`base_item`,`user_id`,`extra_data`) VALUES (@baseItemID, @userID, @extraData)",
				new { baseItemID = baseItemID, userID = userID, extraData = extraData }
			);
			return Convert.ToInt32(InsertQuery);
		}

	}
}
