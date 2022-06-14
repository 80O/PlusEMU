using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class IpBanCommand : IChatCommand
{
    private readonly IDatabase _database;
    private readonly IModerationManager _moderationManager;
    private readonly IGameClientManager _gameClientManager;
    public string Key => "ipban";
    public string PermissionRequired => "command_ip_ban";

    public string Parameters => "%username%";

    public string Description => "IP and account ban another user.";

    public IpBanCommand(IDatabase database, IModerationManager moderationManager, IGameClientManager gameClientManager)
    {
        _database = database;
        _moderationManager = moderationManager;
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
            return;
        }
        var habbo = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1])?.GetHabbo();
        if (habbo == null)
        {
            session.SendWhisper("An error occoured whilst finding that user in the database.");
            return;
        }
        if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
        {
            session.SendWhisper("Oops, you cannot ban that user.");
            return;
        }
        var ipAddress = string.Empty;
        var expire = UnixTimestamp.GetNow() + 78892200;
        var username = habbo.Username;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
            dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + habbo.Id + "' LIMIT 1");
            ipAddress = dbClient.GetString();
        }
        string reason = null;
        if (@params.Length >= 3)
            reason = CommandManager.MergeParams(@params, 2);
        else
            reason = "No reason specified.";
        if (!string.IsNullOrEmpty(ipAddress))
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipAddress, reason, expire);
        _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);
        var targetClient = _gameClientManager.GetClientByUsername(username);
        if (targetClient != null)
            targetClient.Disconnect();
        session.SendWhisper("Success, you have IP and account banned the user '" + username + "' for '" + reason + "'!");
    }
}