﻿using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    class GetCatalogOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var offerId = packet.PopInt();
            if (!PlusEnvironment.GetGame().GetCatalog().ItemOffers.ContainsKey(offerId))
                return;

            var pageId = PlusEnvironment.GetGame().GetCatalog().ItemOffers[offerId];

            if (!PlusEnvironment.GetGame().GetCatalog().TryGetPage(pageId, out var page))
                return;

            if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || (page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1))
                return;

            if (!page.ItemOffers.ContainsKey(offerId))
                return;

            var item = page.ItemOffers[offerId];
            if (item != null)
                session.SendPacket(new CatalogOfferComposer(item));
        }
    }
}
