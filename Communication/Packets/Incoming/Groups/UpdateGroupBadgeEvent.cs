using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateGroupBadgeEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;

    public UpdateGroupBadgeEvent(IGroupManager groupManager, IDatabase database)
    {
        _groupManager = groupManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id)
            return Task.CompletedTask;
        var count = packet.PopInt();
        var badge = "";
        for (var i = 0; i < count; i++) badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.PopInt().ToString(), packet.PopInt().ToString(), packet.PopInt().ToString());
        group.Badge = string.IsNullOrWhiteSpace(badge) ? "b05114s06114" : badge;
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `groups` SET `badge` = @badge WHERE `id` = @groupId LIMIT 1", new { badge = group.Badge, groupId = group.Id });
        }
        session.SendPacket(new GroupInfoComposer(group, session));
        return Task.CompletedTask;
    }
}