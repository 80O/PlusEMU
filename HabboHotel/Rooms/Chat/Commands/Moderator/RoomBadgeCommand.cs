using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class RoomBadgeCommand : IChatCommand
{
    private readonly IBadgeManager _badgeManager;
    public string Key => "roombadge";
    public string PermissionRequired => "command_room_badge";

    public string Parameters => "%badge%";

    public string Description => "Give a badge to the entire room!";

    public RoomBadgeCommand(IBadgeManager badgeManager)
    {
        _badgeManager = badgeManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the name of the badge you'd like to give to the room.");
            return;
        }
        foreach (var user in room.GetRoomUserManager().GetUserList().ToList())
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                continue;
            if (!user.GetClient().GetHabbo().Inventory.Badges.HasBadge(@params[1]))
            {
                _badgeManager.GiveBadge(user.GetClient().GetHabbo(), @params[1]).Wait();
                user.GetClient().SendNotification("You have just been given a badge!");
            }
            else
                user.GetClient().SendWhisper(session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
        }
        session.SendWhisper("You have successfully given every user in this room the " + @params[2] + " badge!");
    }
}