using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Groups;

public interface IGroupManager
{
    ICollection<GroupBadgeParts> BadgeBases { get; }
    ICollection<GroupBadgeParts> BadgeSymbols { get; }
    ICollection<GroupColours> BadgeBaseColours { get; }
    ICollection<GroupColours> BadgeSymbolColours { get; }
    ICollection<GroupColours> BadgeBackColours { get; }
    void Init();
    bool TryGetGroup(int id, out Group group);
    bool TryCreateGroup(Habbo player, string name, string description, int roomId, string badge, int colour1, int colour2, out Group group);
    string GetColourCode(int id, bool colourOne);
    void DeleteGroup(int id);
    List<Group> GetGroupsForUser(int userId);
    Dictionary<int, string> GetAllBadgesInRoom(Room room);
}