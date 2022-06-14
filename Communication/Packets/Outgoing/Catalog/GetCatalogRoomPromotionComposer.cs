using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Catalog;

internal class GetCatalogRoomPromotionComposer : ServerPacket
{
    public GetCatalogRoomPromotionComposer(List<RoomData> usersRooms)
        : base(ServerPacketHeader.PromotableRoomsMessageComposer)
    {
        WriteBoolean(true); //wat
        WriteInteger(usersRooms.Count); //Count of rooms?
        foreach (var room in usersRooms)
        {
            WriteInteger(room.Id);
            WriteString(room.Name);
            WriteBoolean(true);
        }
    }
}