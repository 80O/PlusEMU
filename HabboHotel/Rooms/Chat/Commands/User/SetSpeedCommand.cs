using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class SetSpeedCommand : IChatCommand
{
    public string Key => "setspeed";
    public string PermissionRequired => "command_setspeed";

    public string Parameters => "%value%";

    public string Description => "Set the speed of the rollers in the current room.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!room.CheckRights(session, true))
            return;
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter a value for the roller speed.");
            return;
        }
        int speed;
        if (int.TryParse(parameters[0], out speed))
            session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
        else
            session.SendWhisper("Invalid amount, please enter a valid number.");
    }
}