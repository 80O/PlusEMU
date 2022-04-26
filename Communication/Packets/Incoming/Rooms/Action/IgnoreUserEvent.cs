using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class IgnoreUserEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;
    private readonly IDatabase _database;

    public IgnoreUserEvent(IAchievementManager achievementManager, IDatabase database)
    {
        _achievementManager = achievementManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var username = packet.PopString();
        var player = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo();
        if (player == null || player.GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        if (session.GetHabbo().GetIgnores().TryGet(player.Id))
            return Task.CompletedTask;
        if (session.GetHabbo().GetIgnores().TryAdd(player.Id))
        {
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_ignores` (`user_id`,`ignore_id`) VALUES(@uid,@ignoreId);");
                dbClient.AddParameter("uid", session.GetHabbo().Id);
                dbClient.AddParameter("ignoreId", player.Id);
                dbClient.RunQuery();
            }
            session.SendPacket(new IgnoreStatusComposer(1, player.Username));
            _achievementManager.ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
        }
        return Task.CompletedTask;
    }
}