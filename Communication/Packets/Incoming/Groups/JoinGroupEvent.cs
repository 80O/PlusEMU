using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class JoinGroupEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IGameClientManager _clientManager;

    public JoinGroupEvent(IGroupManager groupManager, IGameClientManager clientManager)
    {
        _groupManager = groupManager;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_groupManager.TryGetGroup(packet.PopInt(), out var group))
            return Task.CompletedTask;
        if (group.IsMember(session.GetHabbo().Id) || group.IsAdmin(session.GetHabbo().Id) || group.HasRequest(session.GetHabbo().Id) && group.Type == GroupType.Private)
            return Task.CompletedTask;
        var groups = _groupManager.GetGroupsForUser(session.GetHabbo().Id);
        if (groups.Count >= 1500)
        {
            session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
            return Task.CompletedTask;
        }
        group.AddMember(session.GetHabbo().Id);
        if (group.Type == GroupType.Locked)
        {
            var groupAdmins = (from client in _clientManager.GetClients.ToList()
                where client != null && client.GetHabbo() != null && @group.IsAdmin(client.GetHabbo().Id)
                select client).ToList();
            foreach (var client in groupAdmins) client.SendPacket(new GroupMembershipRequestedComposer(group.Id, session.GetHabbo(), 3));
            session.SendPacket(new GroupInfoComposer(group, session));
        }
        else
        {
            session.SendPacket(new GroupFurniConfigComposer(_groupManager.GetGroupsForUser(session.GetHabbo().Id)));
            session.SendPacket(new GroupInfoComposer(group, session));
            if (session.GetHabbo().CurrentRoom != null)
                session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            else
                session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
        }
        return Task.CompletedTask;
    }
}