using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetPromotableRoomsEvent : IPacketEvent
{
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var rooms = RoomDataLoader.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
        rooms = rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < UnixTimestamp.GetNow()).ToList();
        session.Send(new PromotableRoomsComposer(rooms));
        return Task.CompletedTask;
    }
}