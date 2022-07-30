using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class BanCommand : ITargetChatCommand
{
    private readonly IDatabase _database;
    private readonly IModerationManager _moderationManager;
    public string Key => "ban";
    public string PermissionRequired => "command_ban";

    public string Parameters => "%username% %length% %reason% ";

    public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

    public bool MustBeInSameRoom => false;

    public BanCommand(IDatabase database, IModerationManager moderationManager)
    {
        _database = database;
        _moderationManager = moderationManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
        {
            session.SendWhisper("Oops, you cannot ban that user.");
            return Task.CompletedTask;
        }
        double expire = 0;
        var hours = parameters[0];
        if (string.IsNullOrEmpty(hours) || hours == "perm")
            expire = UnixTimestamp.GetNow() + 78892200;
        else
            expire = UnixTimestamp.GetNow() + Convert.ToDouble(hours) * 3600;
        string reason;
        if (parameters.Length >= 2)
            reason = CommandManager.MergeParams(parameters, 1);
        else
            reason = "No reason specified.";
        var username = target.Username;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '{target.Id}' LIMIT 1");
        }
        _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, target.Username, reason, expire);
        target.GetClient().Disconnect();
        session.SendWhisper($"Success, you have account banned the user '{username}' for {hours} hour(s) with the reason '{reason}'!");
        return Task.CompletedTask;
    }
}