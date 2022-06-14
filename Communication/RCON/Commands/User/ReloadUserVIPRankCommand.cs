namespace Plus.Communication.Rcon.Commands.User;

internal class ReloadUserVipRankCommand : IRconCommand
{
    public string Description => "This command is used to reload a users VIP rank and permissions.";

    public string Key => "reload_user_vip_rank";
    public string Parameters => "%userId%";

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `rank_vip` FROM `users` WHERE `id` = @userId LIMIT 1");
            dbClient.AddParameter("userId", userId);
            client.GetHabbo().VipRank = dbClient.GetInteger();
        }
        client.GetHabbo().GetPermissions().Init(client.GetHabbo());
        return Task.FromResult(true);
    }
}