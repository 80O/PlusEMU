using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Core.Language;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class GetModeratorUserInfoEvent : IPacketEvent
{
    private readonly ILanguageManager _languageManager;
    private readonly IDatabase _database;

    public GetModeratorUserInfoEvent(ILanguageManager languageManager, IDatabase database)
    {
        _languageManager = languageManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        DataRow user;
        DataRow info;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `id`,`username`,`online`,`mail`,`ip_last`,`look`,`account_created`,`last_online` FROM `users` WHERE `id` = '" + userId + "' LIMIT 1");
            user = dbClient.GetRow();
            if (user == null)
            {
                session.SendNotification(_languageManager.TryGetValue("user.not_found"));
                return Task.CompletedTask;
            }
            dbClient.SetQuery("SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
            info = dbClient.GetRow();
            if (info == null)
            {
                dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
                dbClient.SetQuery("SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                info = dbClient.GetRow();
            }
        }
        session.SendPacket(new ModeratorUserInfoComposer(user, info));
        return Task.CompletedTask;
    }
}