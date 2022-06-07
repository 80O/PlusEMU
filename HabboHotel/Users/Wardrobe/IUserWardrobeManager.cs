using System.Threading.Tasks;


namespace Plus.HabboHotel.Users.Wardrobe
{
	public interface IUserWardrobeManager
	{
		Task SaveOutfit(int userID, int slotID, string gender, string look);
        Task RemoveOutfit(int userID, int slotID);
	}
}
