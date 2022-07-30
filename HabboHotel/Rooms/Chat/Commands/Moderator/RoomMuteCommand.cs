using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class RoomMuteCommand : IChatCommand
{
    public string Key => "roommute";
    public string PermissionRequired => "command_roommute";

    public string Parameters => "%message%";

    public string Description => "Mute the room with a reason.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var message = CommandManager.MergeParams(parameters, 1);
        if (string.IsNullOrWhiteSpace(message))
        {
            session.SendWhisper("Please provide a reason for muting the room to show to the users.");
            return;
        }
        if (!room.RoomMuted)
            room.RoomMuted = true;
        var roomUsers = room.GetRoomUserManager().GetRoomUsers();
        if (roomUsers.Count > 0)
        {
            var whisperMessage = "This room has been muted because: " + message;
            foreach (var user in roomUsers)
            {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Username == session.GetHabbo().Username)
                    continue;
                user.GetClient().SendWhisper(whisperMessage);
            }
        }
    }
}