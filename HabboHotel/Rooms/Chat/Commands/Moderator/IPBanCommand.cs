﻿using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

internal class IpBanCommand : ITargetChatCommand
{
    private readonly IDatabase _database;
    private readonly IModerationManager _moderationManager;
    public string Key => "ipban";
    public string PermissionRequired => "command_ip_ban";

    public string Parameters => "%username%";

    public string Description => "IP and account ban another user.";

    public bool MustBeInSameRoom => true;

    public IpBanCommand(IDatabase database, IModerationManager moderationManager)
    {
        _database = database;
        _moderationManager = moderationManager;
    }

    public Task Execute(GameClient session, Room room, Habbo target, string[] parameters)
    {
        if (target.Permissions.HasRight("mod_tool") && !session.GetHabbo().Permissions.HasRight("mod_ban_any"))
        {
            session.SendWhisper("Oops, you cannot ban that user.");
            return Task.CompletedTask;
        }
        var ipAddress = string.Empty;
        var expire = UnixTimestamp.GetNow() + 78892200;
        var username = target.Username;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + target.Id + "' LIMIT 1");
            dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + target.Id + "' LIMIT 1");
            ipAddress = dbClient.GetString();
        }
        string reason;
        if (parameters.Any())
            reason = CommandManager.MergeParams(parameters);
        else
            reason = "No reason specified.";
        if (!string.IsNullOrEmpty(ipAddress))
            _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipAddress, reason, expire);
        _moderationManager.BanUser(session.GetHabbo().Username, ModerationBanType.Username, target.Username, reason, expire);
        target.Client.Disconnect();
        session.SendWhisper("Success, you have IP and account banned the user '" + username + "' for '" + reason + "'!");
        return Task.CompletedTask;
    }
}