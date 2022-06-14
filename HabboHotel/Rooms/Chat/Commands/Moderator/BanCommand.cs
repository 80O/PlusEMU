using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class BanCommand : IChatCommand
{
    private readonly IDatabase _database;
    private readonly IModerationManager _moderationManager;
    private readonly IGameClientManager _gameClientManager;
    public string Key => "ban";
    public string PermissionRequired => "command_ban";

    public string Parameters => "%username% %length% %reason% ";

    public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

    public BanCommand(IDatabase database, IModerationManager moderationManager, IGameClientManager gameClientManager)
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
        if (habbo.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
        {
            session.SendWhisper("Oops, you cannot ban that user.");
            return;
        }
        double expire = 0;
        var hours = @params[2];
        if (string.IsNullOrEmpty(hours) || hours == "perm")
            expire = UnixTimestamp.GetNow() + 78892200;
        else
            expire = UnixTimestamp.GetNow() + Convert.ToDouble(hours) * 3600;
        string reason = null;
        if (@params.Length >= 4)
            reason = CommandManager.MergeParams(@params, 3);
        else
            reason = "No reason specified.";
        var username = habbo.Username;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + habbo.Id + "' LIMIT 1");
        }
        _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);
        var targetClient = _gameClientManager.GetClientByUsername(username);
        if (targetClient != null)
            targetClient.Disconnect();
        session.SendWhisper("Success, you have account banned the user '" + username + "' for " + hours + " hour(s) with the reason '" + reason + "'!");
    }
}