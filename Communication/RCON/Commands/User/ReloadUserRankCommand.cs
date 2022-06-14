using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Rcon.Commands.User;

internal class ReloadUserRankCommand : IRconCommand
{
    public string Description => "This command is used to reload a users rank and permissions.";

    public string Key => "reload_user_rank";
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
            dbClient.SetQuery("SELECT `rank` FROM `users` WHERE `id` = @userId LIMIT 1");
            dbClient.AddParameter("userId", userId);
            client.GetHabbo().Rank = dbClient.GetInteger();
        }
        client.GetHabbo().GetPermissions().Init(client.GetHabbo());
        if (client.GetHabbo().GetPermissions().HasRight("mod_tickets"))
        {
            client.SendPacket(new ModeratorInitComposer(
                PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                PlusEnvironment.GetGame().GetModerationManager().GetTickets));
        }
        return Task.FromResult(true);
    }
}