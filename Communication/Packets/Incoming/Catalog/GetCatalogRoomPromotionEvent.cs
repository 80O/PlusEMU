using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetCatalogRoomPromotionEvent : IPacketEvent
{
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var rooms = RoomDataLoader.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
        session.Send(new GetCatalogRoomPromotionComposer(rooms));
        return Task.CompletedTask;
    }
}