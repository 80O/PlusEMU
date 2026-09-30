using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetClubOffersEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;

    public GetClubOffersEvent(ICatalogManager catalogManager)
    {
        _catalogManager = catalogManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var source = packet.ReadInt();
        session.Send(new HabboClubOffersComposer(_catalogManager.ClubOffers, source));
        return Task.CompletedTask;
    }
}
