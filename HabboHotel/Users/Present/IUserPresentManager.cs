using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Present
{
	public interface IUserPresentManager
	{
		Task<int> CreatePresent(int itemID, int baseID, string itemExtraData);
	}
}

