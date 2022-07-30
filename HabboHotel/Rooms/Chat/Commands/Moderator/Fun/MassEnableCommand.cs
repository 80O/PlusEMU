using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class MassEnableCommand : IChatCommand
{
    public string Key => "massenable";
    public string PermissionRequired => "command_massenable";

    public string Parameters => "%EffectId%";

    public string Description => "Give every user in the room a specific enable ID.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter an effect ID.");
            return;
        }
        if (int.TryParse(parameters[0], out var enableId))
        {
            if (enableId == 102 || enableId == 178)
            {
                session.Disconnect();
                return;
            }
            if (!session.GetHabbo().GetPermissions().HasCommand("command_override_massenable") && room.OwnerId != session.GetHabbo().Id)
            {
                session.SendWhisper("You can only use this command in your own room.");
                return;
            }
            var users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
            {
                foreach (var u in users.ToList())
                {
                    if (u == null || u.RidingHorse)
                        continue;
                    u.ApplyEffect(enableId);
                }
            }
        }
        else
            session.SendWhisper("Please enter an effect ID.");
    }
}