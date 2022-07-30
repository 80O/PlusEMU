using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class GiveCommand : ITargetChatCommand
{
    public string Key => "give";
    public string PermissionRequired => "command_give";

    public string Parameters => "%username% %type% %amount%";

    public string Description => "";

    public bool MustBeInSameRoom => false;

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendWhisper("Please enter a currency type! (coins, duckets, diamonds, gotw)");
            return Task.CompletedTask;
        }

        var updateVal = parameters[1];
        switch (updateVal.ToLower())
        {
            case "coins":
            case "credits":
            {
                if (!session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                {
                    session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                    break;
                }
                int amount;
                if (int.TryParse(parameters[2], out amount))
                {
                    target.Credits = target.Credits += amount;
                    target.GetClient().Send(new CreditBalanceComposer(target.Credits));
                    if (target.Id != session.GetHabbo().Id)
                        target.GetClient().SendNotification(session.GetHabbo().Username + " has given you " + amount + " Credit(s)!");
                    session.SendWhisper("Successfully given " + amount + " Credit(s) to " + target.Username + "!");
                    break;
                }
                session.SendWhisper("Oops, that appears to be an invalid amount!");
                break;
            }
            case "pixels":
            case "duckets":
            {
                if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                {
                    session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                    break;
                }
                int amount;
                if (int.TryParse(parameters[2], out amount))
                {
                    target.Duckets += amount;
                    target.GetClient().Send(new HabboActivityPointNotificationComposer(target.Duckets, amount));
                    if (target.Id != session.GetHabbo().Id)
                        target.GetClient().SendNotification(session.GetHabbo().Username + " has given you " + amount + " Ducket(s)!");
                    session.SendWhisper("Successfully given " + amount + " Ducket(s) to " + target.Username + "!");
                    break;
                }
                session.SendWhisper("Oops, that appears to be an invalid amount!");
                break;
            }
            case "diamonds":
            {
                if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                {
                    session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                    break;
                }
                int amount;
                if (int.TryParse(parameters[2], out amount))
                {
                    target.Diamonds += amount;
                    target.GetClient().Send(new HabboActivityPointNotificationComposer(target.Diamonds, amount, 5));
                    if (target.Id != session.GetHabbo().Id)
                        target.GetClient().SendNotification(session.GetHabbo().Username + " has given you " + amount + " Diamond(s)!");
                    session.SendWhisper("Successfully given " + amount + " Diamond(s) to " + target.Username + "!");
                    break;
                }
                session.SendWhisper("Oops, that appears to be an invalid amount!");
                break;
            }
            case "gotw":
            case "gotwpoints":
            {
                if (!session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                {
                    session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                    break;
                }
                int amount;
                if (int.TryParse(parameters[2], out amount))
                {
                    target.GotwPoints = target.GotwPoints + amount;
                    target.GetClient().Send(new HabboActivityPointNotificationComposer(target.GotwPoints, amount, 103));
                    if (target.Id != session.GetHabbo().Id)
                        target.GetClient().SendNotification(session.GetHabbo().Username + " has given you " + amount + " GOTW Point(s)!");
                    session.SendWhisper("Successfully given " + amount + " GOTW point(s) to " + target.Username + "!");
                    break;
                }
                session.SendWhisper("Oops, that appears to be an invalid amount!");
                break;
            }
            default:
                session.SendWhisper("'" + updateVal + "' is not a valid currency!");
                break;
        }
        return Task.CompletedTask;
    }
}