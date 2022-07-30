using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class GotoCommand : IChatCommand
{
    public string Key => "goto";
    public string PermissionRequired => "command_goto";

    public string Parameters => "%room_id%";

    public string Description => "";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("You must specify a room id!");
            return;
        }
        if (!int.TryParse(parameters[0], out var roomId))
            session.SendWhisper("You must enter a valid room ID");
        else
        {
            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                session.SendWhisper("This room does not exist!");
                return;
            }
            session.GetHabbo().PrepareRoom(roomId, "");
        }
    }
}