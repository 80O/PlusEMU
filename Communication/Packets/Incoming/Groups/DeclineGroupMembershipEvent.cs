using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class DeclineGroupMembershipEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public DeclineGroupMembershipEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        var userId = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (session.GetHabbo().Id != group.CreatorId && !group.IsAdmin(session.GetHabbo().Id))
            return Task.CompletedTask;
        if (!group.HasRequest(userId))
            return Task.CompletedTask;
        group.HandleRequest(userId, false);
        session.SendPacket(new UnknownGroupComposer(group.Id, userId));
        return Task.CompletedTask;
    }
}