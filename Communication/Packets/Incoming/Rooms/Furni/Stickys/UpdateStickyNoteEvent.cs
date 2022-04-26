using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys;

internal class UpdateStickyNoteEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public UpdateStickyNoteEvent(IRoomManager roomManager)
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
        var color = packet.PopString();
        var text = packet.PopString();
        if (!room.CheckRights(session))
        {
            if (!text.StartsWith(item.ExtraData))
                return Task.CompletedTask; // we can only ADD stuff! older stuff changed, this is not allowed
        }
        switch (color)
        {
            case "FFFF33":
            case "FF9CFF":
            case "9CCEFF":
            case "9CFF9C":
                break;
            default:
                return Task.CompletedTask; // invalid color
        }
        item.ExtraData = color + " " + text;
        item.UpdateState(true, true);
        return Task.CompletedTask;
    }
}