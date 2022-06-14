using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.Communication.Rcon.Commands.User;

internal class ReloadUserCurrencyCommand : IRconCommand
{
    public string Description => "This command is used to update the users currency from the database.";

    public string Key => "reload_user_currency";
    public string Parameters => "%userId% %currency%";

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);

        // Validate the currency type
        if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            return Task.FromResult(false);
        var currency = Convert.ToString(parameters[1]);
        switch (currency)
        {
            default:
                return Task.FromResult(false);
            case "coins":
            case "credits":
                {
                    int credits;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `credits` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", userId);
                        credits = dbClient.GetInteger();
                    }
                    client.GetHabbo().Credits = credits;
                    client.SendPacket(new CreditBalanceComposer(client.GetHabbo().Credits));
                    break;
                }
            case "pixels":
            case "duckets":
                {
                    int duckets;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `activity_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", userId);
                        duckets = dbClient.GetInteger();
                    }
                    client.GetHabbo().Duckets = duckets;
                    client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().Duckets, duckets));
                    break;
                }
            case "diamonds":
                {
                    int diamonds;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `vip_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", userId);
                        diamonds = dbClient.GetInteger();
                    }
                    client.GetHabbo().Diamonds = diamonds;
                    client.SendPacket(new HabboActivityPointNotificationComposer(diamonds, 0, 5));
                    break;
                }
            case "gotw":
                {
                    int gotw;
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `gotw_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", userId);
                        gotw = dbClient.GetInteger();
                    }
                    client.GetHabbo().GotwPoints = gotw;
                    client.SendPacket(new HabboActivityPointNotificationComposer(gotw, 0, 103));
                    break;
                }
        }
        return Task.FromResult(true);
    }
}