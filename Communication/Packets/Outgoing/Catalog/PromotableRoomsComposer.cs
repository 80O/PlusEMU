using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Catalog;

internal class PromotableRoomsComposer : ServerPacket
{
    public PromotableRoomsComposer(ICollection<RoomData> rooms)
        : base(ServerPacketHeader.PromotableRoomsMessageComposer)
    {
        WriteBoolean(true);
        WriteInteger(rooms.Count); //Count
        foreach (var data in rooms)
        {
            WriteInteger(data.Id);
            WriteString(data.Name);
            WriteBoolean(false);
        }
    }
}