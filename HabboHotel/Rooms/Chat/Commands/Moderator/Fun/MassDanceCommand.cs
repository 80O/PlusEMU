using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class MassDanceCommand : IChatCommand
{
    public string Key => "massdance";
    public string PermissionRequired => "command_massdance";

    public string Parameters => "%DanceId%";

    public string Description => "Force everyone in the room to dance to a dance of your choice.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter a dance ID. (1-4)");
            return;
        }
        var danceId = Convert.ToInt32(parameters[0]);
        if (danceId < 0 || danceId > 4)
        {
            session.SendWhisper("Please enter a dance ID. (1-4)");
            return;
        }
        var users = room.GetRoomUserManager().GetRoomUsers();
        if (users.Count > 0)
        {
            foreach (var u in users.ToList())
            {
                if (u == null)
                    continue;
                if (u.CarryItemId > 0)
                    u.CarryItemId = 0;
                u.DanceId = danceId;
                room.SendPacket(new DanceComposer(u, danceId));
            }
        }
    }
}