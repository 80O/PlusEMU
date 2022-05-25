using System.Threading.Tasks;

namespace Plus.HabboHotel.Catalog.Item
{
	public interface ICatalogItemManager
	{
		Task UpdateItem(int catalogItemID, UpdateCatalogItemDTO updateCatalogItemDTO);
	}
}

