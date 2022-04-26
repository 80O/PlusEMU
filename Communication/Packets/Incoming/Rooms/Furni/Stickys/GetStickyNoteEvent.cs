using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Stickys;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys;

internal class GetStickyNoteEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public GetStickyNoteEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
        if (item == null || item.GetBaseItem().InteractionType != InteractionType.Postit)
            return Task.CompletedTask;
        session.SendPacket(new StickyNoteComposer(item.Id.ToString(), item.ExtraData));
        return Task.CompletedTask;
    }
}