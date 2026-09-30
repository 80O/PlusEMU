using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys;

// TODO @80O: Verify stickies get the owner of the rooms recipient
internal class AddStickyNoteEvent : RoomPacketEvent
{
    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        lock (session.GetHabbo().Inventory.Furniture.SyncRoot)
        {
            var itemId = packet.ReadUInt();
            var locationData = packet.ReadString();
            if (!room.CheckRights(session))
                return Task.CompletedTask;
            var item = session.GetHabbo().Inventory.Furniture.GetItem(itemId);
            if (item == null)
                return Task.CompletedTask;
            try
            {
                var wallPossition = room.GetRoomItemHandler().WallPositionCheck($":{locationData.Split(':')[1]}");
                var roomItem = item.ToRoomObject();
                roomItem.WallCoordinates = wallPossition;
                if (room.GetRoomItemHandler().SetWallItem(session, roomItem))
                {
                    session.GetHabbo().Inventory.Furniture.RemoveItem(itemId);
                    session.Send(new FurniListRemoveComposer(itemId));
                }
            }
            catch
            {
                //TODO: Send a packet
            }
            return Task.CompletedTask;
        }
    }
}
