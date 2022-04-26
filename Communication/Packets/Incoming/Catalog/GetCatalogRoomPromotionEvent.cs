using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetCatalogRoomPromotionEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id);
        session.SendPacket(new GetCatalogRoomPromotionComposer(rooms));
        return Task.CompletedTask;
    }
}