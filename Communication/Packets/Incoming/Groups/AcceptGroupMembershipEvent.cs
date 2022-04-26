using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class AcceptGroupMembershipEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public AcceptGroupMembershipEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        var userId = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (session.GetHabbo().Id != group.CreatorId && !group.IsAdmin(session.GetHabbo().Id) && !session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
            return Task.CompletedTask;
        if (!group.HasRequest(userId))
            return Task.CompletedTask;
        var habbo = PlusEnvironment.GetHabboById(userId);
        if (habbo == null)
        {
            session.SendNotification("Oops, an error occurred whilst finding this user.");
            return Task.CompletedTask;
        }
        group.HandleRequest(userId, true);
        session.SendPacket(new GroupMemberUpdatedComposer(groupId, habbo, 4));
        return Task.CompletedTask;
    }
}