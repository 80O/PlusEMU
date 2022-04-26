using System;
using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class CancelOfferEvent : IPacketEvent
{
    private readonly IItemDataManager _itemDataManager;
    private readonly IDatabase _database;

    public CancelOfferEvent(IItemDataManager itemDataManager, IDatabase database)
    {
        _itemDataManager = itemDataManager;
        _database = database;
    }
    public Task Parse(GameClient session, ClientPacket packet)
    {
        DataRow row;
        var offerId = packet.PopInt();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery(
                "SELECT `furni_id`, `item_id`, `user_id`, `extra_data`, `offer_id`, `state`, `timestamp`, `limited_number`, `limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
            dbClient.AddParameter("OfferId", offerId);
            row = dbClient.GetRow();
        }
        if (row == null)
        {
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
            return Task.CompletedTask;
        }
        if (Convert.ToInt32(row["user_id"]) != session.GetHabbo().Id)
        {
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
            return Task.CompletedTask;
        }
        if (!_itemDataManager.GetItem(Convert.ToInt32(row["item_id"]), out var item))
        {
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
            return Task.CompletedTask;
        }

        //PlusEnvironment.GetGame().GetCatalog().DeliverItems(Session, Item, 1, Convert.ToString(Row["extra_data"]), Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]), Convert.ToInt32(Row["furni_id"]));
        var giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), Convert.ToString(row["extra_data"]), Convert.ToString(row["extra_data"]), Convert.ToInt32(row["furni_id"]),
            Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
        session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
        session.SendPacket(new FurniListUpdateComposer());
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("DELETE FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId AND `user_id` = @UserId LIMIT 1");
            dbClient.AddParameter("OfferId", offerId);
            dbClient.AddParameter("UserId", session.GetHabbo().Id);
            dbClient.RunQuery();
        }
        session.GetHabbo().GetInventoryComponent().UpdateItems(true);
        session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, true));
        return Task.CompletedTask;
    }
}