using System.Threading.Tasks;

namespace Plus.HabboHotel.Items
{
	public interface IItemManager
	{
		Task<int> CreateItem(int userID, int baseItemID, string extraData);
	}
}
