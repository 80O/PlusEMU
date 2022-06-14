using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class AllEyesOnMeCommand : IChatCommand
{
    public string Key => "alleyesonme";
    public string PermissionRequired => "command_alleyesonme";

    public string Parameters => "";

    public string Description => "Want some attention? Make everyone face you!";

    public void Execute(GameClient session, Room room, string[] @params)
    {
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return;
        var users = room.GetRoomUserManager().GetRoomUsers();
        foreach (var u in users.ToList())
        {
            if (u == null || session.GetHabbo().Id == u.UserId)
                continue;
            u.SetRot(Rotation.Calculate(u.X, u.Y, thisUser.X, thisUser.Y), false);
        }
    }
}