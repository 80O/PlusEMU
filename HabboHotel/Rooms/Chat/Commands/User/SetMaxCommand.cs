using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class SetMaxCommand : IChatCommand
{
    private readonly IDatabase _database;
    public string Key => "setmax";
    public string PermissionRequired => "command_setmax";

    public string Parameters => "%value%";

    public string Description => "Set the visitor limit to the room.";

    public SetMaxCommand(IDatabase database)
    {
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!room.CheckRights(session, true))
            return;
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter a value for the room visitor limit.");
            return;
        }
        int maxAmount;
        if (int.TryParse(parameters[0], out maxAmount))
        {
            if (maxAmount == 0)
            {
                maxAmount = 10;
                session.SendWhisper("visitor amount too low, visitor amount has been set to 10.");
            }
            else if (maxAmount > 200 && !session.GetHabbo().GetPermissions().HasRight("override_command_setmax_limit"))
            {
                maxAmount = 200;
                session.SendWhisper("visitor amount too high for your rank, visitor amount has been set to 200.");
            }
            else
                session.SendWhisper("visitor amount set to " + maxAmount + ".");
            room.UsersMax = maxAmount;
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery("UPDATE `rooms` SET `users_max` = " + maxAmount + " WHERE `id` = '" + room.Id + "' LIMIT 1");
        }
        else
            session.SendWhisper("Invalid amount, please enter a valid number.");
    }
}