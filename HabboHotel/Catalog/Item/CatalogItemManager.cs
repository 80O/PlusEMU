using Dapper;
using Plus.Database;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Plus.HabboHotel.Catalog.Item
{
	public class CatalogItemManager : ICatalogItemManager
	{

		private readonly IDatabase _database;

		public CatalogItemManager(IDatabase database)
		{
			_database = database;
		}

		public async Task UpdateItem(int catalogItemID, UpdateCatalogItemDTO updateCatalogItemDTO)
        {
			List<string> catalogItemChanges = new List<string>();

			PropertyInfo[] updateCatalogItemDTOProperties = typeof(UpdateCatalogItemDTO).GetProperties();

			foreach (PropertyInfo updateCatalogItemDTOProperty in updateCatalogItemDTOProperties)
			{
				string updatedCatalogItemColumn = updateCatalogItemDTOProperty.ToString();
				string updatedCatalogItemValue = updateCatalogItemDTOProperty.GetValue(updateCatalogItemDTO).ToString();
				catalogItemChanges.Add($"{updatedCatalogItemColumn}={updatedCatalogItemValue}");
			}

            using var connection = _database.Connection();
			await connection.ExecuteAsync(
				 $"UPDATE `catalog_items` SET {catalogItemChanges} WHERE `id` = @catalogItemID LIMIT 1",
				 new { catalogItemID = catalogItemID }
			);
        }

	}
}