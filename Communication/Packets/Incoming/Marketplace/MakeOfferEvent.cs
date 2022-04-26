using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class MakeOfferEvent : IPacketEvent
{
    private readonly IMarketplaceManager _marketplaceManager;
    private readonly IDatabase _database;

    public MakeOfferEvent(IMarketplaceManager marketplaceManager, IDatabase database)
    {
        _marketplaceManager = marketplaceManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var sellingPrice = packet.PopInt();
        packet.PopInt(); //comission
        var itemId = packet.PopInt();
        var item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);
        if (item == null)
        {
            session.SendPacket(new MarketplaceMakeOfferResultComposer(0));
            return Task.CompletedTask;
        }
        if (!ItemUtility.IsRare(item))
        {
            session.SendNotification("Sorry, only Rares & LTDs can go be auctioned off in the Marketplace!");
            return Task.CompletedTask;
        }
        if (sellingPrice > 70000000 || sellingPrice == 0)
        {
            session.SendPacket(new MarketplaceMakeOfferResultComposer(0));
            return Task.CompletedTask;
        }
        var comission = _marketplaceManager.CalculateComissionPrice(sellingPrice);
        var totalPrice = sellingPrice + comission;
        var itemType = 1;
        if (item.GetBaseItem().Type == 'i')
            itemType++;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery(
                "INSERT INTO `catalog_marketplace_offers` (`furni_id`,`item_id`,`user_id`,`asking_price`,`total_price`,`public_name`,`sprite_id`,`item_type`,`timestamp`,`extra_data`,`limited_number`,`limited_stack`) VALUES ('" +
                itemId + "','" + item.BaseItem + "','" + session.GetHabbo().Id + "','" + sellingPrice + "','" + totalPrice + "',@public_name,'" + item.GetBaseItem().SpriteId + "','" + itemType +
                "','" + UnixTimestamp.GetNow() + "',@extra_data, '" + item.LimitedNo + "', '" + item.LimitedTot + "')");
            dbClient.AddParameter("public_name", item.GetBaseItem().PublicName);
            dbClient.AddParameter("extra_data", item.ExtraData);
            dbClient.RunQuery();
            dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + itemId + "' AND `user_id` = '" + session.GetHabbo().Id + "' LIMIT 1");
        }
        session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
        session.SendPacket(new MarketplaceMakeOfferResultComposer(1));
        return Task.CompletedTask;
    }
}