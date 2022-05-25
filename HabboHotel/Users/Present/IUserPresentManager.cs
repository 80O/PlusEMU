using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Present
{
	public interface IUserPresentManager
	{
		Task GivePresent(int itemID, int baseID, string itemExtraData);
	}
}

