using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine;

internal class RoomEventComposer : ServerPacket
{
    public RoomEventComposer(RoomData data, RoomPromotion promotion)
        : base(ServerPacketHeader.RoomEventMessageComposer)
    {
        WriteInteger(promotion == null ? -1 : Convert.ToInt32(data.Id));
        WriteInteger(promotion == null ? -1 : data.OwnerId);
        WriteString(promotion == null ? "" : data.OwnerName);
        WriteInteger(promotion == null ? 0 : 1);
        WriteInteger(0);
        WriteString(promotion == null ? "" : promotion.Name);
        WriteString(promotion == null ? "" : promotion.Description);
        WriteInteger(0);
        WriteInteger(0);
        WriteInteger(0); //Unknown, came in build RELEASE63-201411181343-400753188
    }
}