using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class UpdateMagicTileEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (!room.CheckRights(session, false, true) && !session.GetHabbo().GetPermissions().HasRight("room_item_use_any_stack_tile"))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var decimalHeight = packet.PopInt();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        item.GetZ = decimalHeight / 100.0;
        room.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(session.GetHabbo().Id)));
        room.SendPacket(new UpdateMagicTileComposer(itemId, decimalHeight));
        return Task.CompletedTask;
    }
}