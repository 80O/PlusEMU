using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities.FigureData.Types;

namespace Plus.HabboHotel.Users.Clothing;
public interface IFigureDataManager
{
    Task InitAsync();
    Task UpdateFigureData();
    Task <string> ValidateLookAsync(string itemString, string gender, ICollection<ClothingParts> clothingParts, bool hasHabboClub);

}