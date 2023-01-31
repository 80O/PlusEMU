﻿using Plus.Communication.Packets.Outgoing.Marketplace;
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
    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var offerId = packet.ReadUInt();
        var success = await _marketplaceManager.TryCancelOffer(session.GetHabbo(), offerId);
        session.Send(new MarketplaceCancelOfferResultComposer(offerId, success));
    }
}