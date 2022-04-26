using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class GetGroupFurniSettingsEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public GetGroupFurniSettingsEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var groupId = packet.PopInt();
        var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        if (item.Data.InteractionType != InteractionType.GuildGate)
            return Task.CompletedTask;
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        session.SendPacket(new GroupFurniSettingsComposer(group, itemId, session.GetHabbo().Id));
        session.SendPacket(new GroupInfoComposer(group, session));
        return Task.CompletedTask;
    }
}