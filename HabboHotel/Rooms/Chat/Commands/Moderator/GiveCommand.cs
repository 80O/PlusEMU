using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class GiveCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "give";
    public string PermissionRequired => "command_give";

    public string Parameters => "%username% %type% %amount%";

    public string Description => "";

    public GiveCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter a currency type! (coins, duckets, diamonds, gotw)");
            return;
        }
        var target = _gameClientManager.GetClientByUsername(@params[1]);
        if (target == null)
        {
            session.SendWhisper("Oops, couldn't find that user!");
            return;
        }
        var updateVal = @params[2];
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
                    if (int.TryParse(@params[3], out amount))
                    {
                        target.GetHabbo().Credits = target.GetHabbo().Credits += amount;
                        target.SendPacket(new CreditBalanceComposer(target.GetHabbo().Credits));
                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " has given you " + amount + " Credit(s)!");
                        session.SendWhisper("Successfully given " + amount + " Credit(s) to " + target.GetHabbo().Username + "!");
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
                    if (int.TryParse(@params[3], out amount))
                    {
                        target.GetHabbo().Duckets += amount;
                        target.SendPacket(new HabboActivityPointNotificationComposer(target.GetHabbo().Duckets, amount));
                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " has given you " + amount + " Ducket(s)!");
                        session.SendWhisper("Successfully given " + amount + " Ducket(s) to " + target.GetHabbo().Username + "!");
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
                    if (int.TryParse(@params[3], out amount))
                    {
                        target.GetHabbo().Diamonds += amount;
                        target.SendPacket(new HabboActivityPointNotificationComposer(target.GetHabbo().Diamonds, amount, 5));
                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " has given you " + amount + " Diamond(s)!");
                        session.SendWhisper("Successfully given " + amount + " Diamond(s) to " + target.GetHabbo().Username + "!");
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
                    if (int.TryParse(@params[3], out amount))
                    {
                        target.GetHabbo().GotwPoints = target.GetHabbo().GotwPoints + amount;
                        target.SendPacket(new HabboActivityPointNotificationComposer(target.GetHabbo().GotwPoints, amount, 103));
                        if (target.GetHabbo().Id != session.GetHabbo().Id)
                            target.SendNotification(session.GetHabbo().Username + " has given you " + amount + " GOTW Point(s)!");
                        session.SendWhisper("Successfully given " + amount + " GOTW point(s) to " + target.GetHabbo().Username + "!");
                        break;
                    }
                    session.SendWhisper("Oops, that appears to be an invalid amount!");
                    break;
                }
            default:
                session.SendWhisper("'" + updateVal + "' is not a valid currency!");
                break;
        }
    }
}