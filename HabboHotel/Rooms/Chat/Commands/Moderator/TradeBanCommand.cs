using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class TradeBanCommand : IChatCommand
{
    private readonly IDatabase _database;
    public string Key => "tradeban";
    public string PermissionRequired => "command_trade_ban";

    public string Parameters => "%target% %length%";

    public string Description => "Trade ban another user.";

    public TradeBanCommand(IDatabase database)
    {
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter a username and a valid length in days (min 1 day, max 365 days).");
            return;
        }
        var habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
        if (habbo == null)
        {
            session.SendWhisper("An error occoured whilst finding that user in the database.");
            return;
        }
        if (Convert.ToDouble(@params[2]) == 0)
        {
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }
            if (habbo.GetClient() != null)
            {
                habbo.TradingLockExpiry = 0;
                habbo.GetClient().SendNotification("Your outstanding trade ban has been removed.");
            }
            session.SendWhisper("You have successfully removed " + habbo.Username + "'s trade ban.");
            return;
        }
        double days;
        if (double.TryParse(@params[2], out days))
        {
            if (days < 1)
                days = 1;
            if (days > 365)
                days = 365;
            var length = PlusEnvironment.GetUnixTimestamp() + days * 86400;
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '" + length + "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            }
            if (habbo.GetClient() != null)
            {
                habbo.TradingLockExpiry = length;
                habbo.GetClient().SendNotification("You have been trade banned for " + days + " day(s)!");
            }
            session.SendWhisper("You have successfully trade banned " + habbo.Username + " for " + days + " day(s).");
        }
        else
            session.SendWhisper("Please enter a valid integer.");
    }
}