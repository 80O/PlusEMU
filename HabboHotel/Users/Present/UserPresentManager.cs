using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Present
{
	public class UserPresentManager : IUserPresentManager
	{
		public UserPresentManager()
		{
		}

		public Task GivePresent(int itemID, int baseID, string itemExtraData)
        {
			return Task.CompletedTask;
        }
	}
}

