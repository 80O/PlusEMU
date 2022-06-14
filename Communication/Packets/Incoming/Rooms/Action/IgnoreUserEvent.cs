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
        if (session.GetHabbo().IgnoresComponent.IsIgnored(player.Id))
            return Task.CompletedTask;
        session.GetHabbo().IgnoresComponent.Ignore(player.Id);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
        return Task.CompletedTask;
    }
}