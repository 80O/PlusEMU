using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class BuyOfferEvent : IPacketEvent
{
    private readonly IMarketplaceManager _marketplace;
    private readonly IItemDataManager _itemDataManager;
    private readonly IDatabase _database;

    public BuyOfferEvent(IMarketplaceManager marketplace, IItemDataManager itemDataManager, IDatabase database)
    {
        _marketplace = marketplace;
        _itemDataManager = itemDataManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var offerId = packet.PopInt();
        DataRow row;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery(
                "SELECT `state`,`timestamp`,`total_price`,`extra_data`,`item_id`,`furni_id`,`user_id`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
            dbClient.AddParameter("OfferId", offerId);
            row = dbClient.GetRow();
        }
        if (row == null)
        {
            ReloadOffers(session);
            return Task.CompletedTask;
        }
        if (Convert.ToString(row["state"]) == "2")
        {
            session.SendNotification("Oops, this offer is no longer available.");
            ReloadOffers(session);
            return Task.CompletedTask;
        }
        if (_marketplace.FormatTimestamp() > Convert.ToDouble(row["timestamp"]))
        {
            session.SendNotification("Oops, this offer has expired..");
            ReloadOffers(session);
            return Task.CompletedTask;
        }
        if (!_itemDataManager.GetItem(Convert.ToInt32(row["item_id"]), out var item))
        {
            session.SendNotification("Item isn't in the hotel anymore.");
            ReloadOffers(session);
            return Task.CompletedTask;
        }
        {
            if (Convert.ToInt32(row["user_id"]) == session.GetHabbo().Id)
            {
                session.SendNotification("To prevent average boosting you cannot purchase your own marketplace offers.");
                return Task.CompletedTask;
            }
            if (Convert.ToInt32(row["total_price"]) > session.GetHabbo().Credits)
            {
                session.SendNotification("Oops, you do not have enough credits for this.");
                return Task.CompletedTask;
            }
            session.GetHabbo().Credits -= Convert.ToInt32(row["total_price"]);
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            var giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), Convert.ToString(row["extra_data"]), Convert.ToString(row["extra_data"]), Convert.ToInt32(row["furni_id"]),
                Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
            if (giveItem != null)
            {
                session.GetHabbo().GetInventoryComponent().TryAddItem(giveItem);
                session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
                session.SendPacket(new PurchaseOkComposer());
                session.SendPacket(new FurniListAddComposer(giveItem));
                session.SendPacket(new FurniListUpdateComposer());
            }
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery("UPDATE `catalog_marketplace_offers` SET `state` = '2' WHERE `offer_id` = '" + offerId + "' LIMIT 1");
            int id;
            dbClient.SetQuery("SELECT `id` FROM `catalog_marketplace_data` WHERE `sprite` = " + item.SpriteId + " LIMIT 1;");
            id = dbClient.GetInteger();
            if (id > 0)
                dbClient.RunQuery("UPDATE `catalog_marketplace_data` SET `sold` = `sold` + 1, `avgprice` = (avgprice + " + Convert.ToInt32(row["total_price"]) + ") WHERE `id` = " + id + " LIMIT 1;");
            else
                dbClient.RunQuery("INSERT INTO `catalog_marketplace_data` (`sprite`, `sold`, `avgprice`) VALUES ('" + item.SpriteId + "', '1', '" + Convert.ToInt32(row["total_price"]) + "')");
            if (_marketplace.MarketAverages.ContainsKey(item.SpriteId) &&
                _marketplace.MarketCounts.ContainsKey(item.SpriteId))
            {
                var num3 = _marketplace.MarketCounts[item.SpriteId];
                var num4 = _marketplace.MarketAverages[item.SpriteId] += Convert.ToInt32(row["total_price"]);
                _marketplace.MarketAverages.Remove(item.SpriteId);
                _marketplace.MarketAverages.Add(item.SpriteId, num4);
                _marketplace.MarketCounts.Remove(item.SpriteId);
                _marketplace.MarketCounts.Add(item.SpriteId, num3 + 1);
            }
            else
            {
                if (!_marketplace.MarketAverages.ContainsKey(item.SpriteId))
                    _marketplace.MarketAverages.Add(item.SpriteId, Convert.ToInt32(row["total_price"]));
                if (!_marketplace.MarketCounts.ContainsKey(item.SpriteId))
                    _marketplace.MarketCounts.Add(item.SpriteId, 1);
            }
        }
        ReloadOffers(session);
        return Task.CompletedTask;
    }


    private void ReloadOffers(GameClient session)
    {
        var minCost = -1;
        var maxCost = -1;
        var searchQuery = "";
        var filterMode = 1;
        DataTable table = null;
        var builder = new StringBuilder();
        string str;
        builder.Append("WHERE `state` = '1' AND `timestamp` >= " + _marketplace.FormatTimestampString());
        if (minCost >= 0) builder.Append(" AND `total_price` > " + minCost);
        if (maxCost >= 0) builder.Append(" AND `total_price` < " + maxCost);
        switch (filterMode)
        {
            case 1:
                str = "ORDER BY `asking_price` DESC";
                break;
            default:
                str = "ORDER BY `asking_price` ASC";
                break;
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `offer_id`,`item_type`,`sprite_id`,`total_price`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` " + builder + " " + str + " LIMIT 500");
            dbClient.AddParameter("search_query", "%" + searchQuery + "%");
            if (searchQuery.Length >= 1) builder.Append(" AND `public_name` LIKE @search_query");
            table = dbClient.GetTable();
        }
        _marketplace.MarketItems.Clear();
        _marketplace.MarketItemKeys.Clear();
        if (table != null)
        {
            foreach (DataRow row in table.Rows)
            {
                if (!_marketplace.MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"])))
                {
                    var item = new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()),
                        Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
                    _marketplace.MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                    _marketplace.MarketItems.Add(item);
                }
            }
        }
        var dictionary = new Dictionary<int, MarketOffer>();
        var dictionary2 = new Dictionary<int, int>();
        foreach (var item in _marketplace.MarketItems)
        {
            if (dictionary.ContainsKey(item.SpriteId))
            {
                if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                {
                    dictionary.Remove(item.SpriteId);
                    dictionary.Add(item.SpriteId, item);
                }
                var num = dictionary2[item.SpriteId];
                dictionary2.Remove(item.SpriteId);
                dictionary2.Add(item.SpriteId, num + 1);
            }
            else
            {
                dictionary.Add(item.SpriteId, item);
                dictionary2.Add(item.SpriteId, 1);
            }
        }
        session.SendPacket(new MarketPlaceOffersComposer(dictionary, dictionary2));
    }
}