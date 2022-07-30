using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class RoomAlertCommand : IChatCommand
{
    public string Key => "roomalert";
    public string PermissionRequired => "command_room_alert";

    public string Parameters => "%message%";

    public string Description => "Send a message to the users in this room.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter a message you'd like to send to the room.");
            return;
        }
        if (!session.GetHabbo().GetPermissions().HasRight("mod_alert") && room.OwnerId != session.GetHabbo().Id)
        {
            session.SendWhisper("You can only Room Alert in your own room!");
            return;
        }
        var message = $"{session.GetHabbo().Username} alerted the room with the following message:\n\n{CommandManager.MergeParams(parameters)}";
        foreach (var roomUser in room.GetRoomUserManager().GetRoomUsers())
        {
            if (roomUser == null || roomUser.GetClient() == null || session.GetHabbo().Id == roomUser.UserId)
                continue;
            roomUser.GetClient().SendNotification(message);
        }
        session.SendWhisper("Message successfully sent to the room.");
    }
}