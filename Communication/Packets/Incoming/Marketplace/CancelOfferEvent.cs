using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class CancelOfferEvent : IPacketEvent
{
    private readonly IMarketplaceManager _marketplaceManager;

    public CancelOfferEvent(IMarketplaceManager marketplaceManager)
    {
        _marketplaceManager = marketplaceManager;
    }
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var offerId = packet.PopInt();
        var success = await _marketplaceManager.TryCancelOffer(session.GetHabbo(), offerId);
        session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, success));
    }
}