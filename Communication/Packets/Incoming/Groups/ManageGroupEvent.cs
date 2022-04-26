using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class ManageGroupEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public ManageGroupEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("group_management_override"))
            return Task.CompletedTask;
        session.SendPacket(new ManageGroupComposer(group, group.Badge.Replace("b", "").Split('s')));
        return Task.CompletedTask;
    }
}