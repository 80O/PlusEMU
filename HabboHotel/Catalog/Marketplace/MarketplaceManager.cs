using Dapper;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Catalog.Marketplace;

public class MarketplaceManager : IMarketplaceManager
{
    private readonly IDatabase _database;
    private readonly IItemDataManager _itemDataManager;
    public Dictionary<int, int> MarketAverages { get; } = new();
    public Dictionary<int, int> MarketCounts { get; } = new();
    public List<int> MarketItemKeys { get; } = new();
    public List<MarketOffer> MarketItems { get; } = new();

    public MarketplaceManager(IDatabase database, IItemDataManager itemDataManager)
    {
        _database = database;
        _itemDataManager = itemDataManager;
    }
    public int AvgPriceForSprite(int spriteId)
    {
        var num = 0;
        var num2 = 0;
        if (MarketAverages.ContainsKey(spriteId) && MarketCounts.ContainsKey(spriteId))
        {
            if (MarketCounts[spriteId] > 0) return MarketAverages[spriteId] / MarketCounts[spriteId];
            return 0;
        }
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = '" + spriteId + "' LIMIT 1");
            num = dbClient.GetInteger();
            dbClient.SetQuery("SELECT `sold` FROM `catalog_marketplace_data` WHERE `sprite` = '" + spriteId + "' LIMIT 1");
            num2 = dbClient.GetInteger();
        }
        MarketAverages.Add(spriteId, num);
        MarketCounts.Add(spriteId, num2);
        if (num2 > 0)
            return Convert.ToInt32(Math.Ceiling((double)(num / num2)));
        return 0;
    }

    public string FormatTimestampString()
    {
        return FormatTimestamp().ToString().Split(new[] { ',' })[0];
    }

    public double FormatTimestamp() => UnixTimestamp.GetNow() - 172800.0;

    public int OfferCountForSprite(int spriteId)
    {
        var dictionary = new Dictionary<int, MarketOffer>();
        var dictionary2 = new Dictionary<int, int>();
        foreach (var item in MarketItems)
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
        if (dictionary2.ContainsKey(spriteId)) return dictionary2[spriteId];
        return 0;
    }

    public int CalculateComissionPrice(float price) => Convert.ToInt32(Math.Ceiling(price / 100 * 1));

    public async Task<bool> TryCancelOffer(Habbo habbo, int offerId)
    {
        var offer = await GetOffer(offerId);
        if (offer?.UserId != habbo.Id) return false;

        var item = _itemDataManager.GetItemData(offer.ItemId);
        if (item == null) return false;

        var giveItem = ItemFactory.CreateSingleItem(item, habbo, offer.ExtraData, offer.ExtraData, offer.FurniId, offer.LimitedNumber, offer.LimitedStack);
        habbo.GetClient().SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
        habbo.GetClient().SendPacket(new FurniListUpdateComposer());
        await DeleteOffer(offerId);
        return true;
    }

    public async Task<MarketOffer?> GetOffer(int offerId)
    {
        using var connection = _database.Connection();
        return await connection.QuerySingleOrDefaultAsync<MarketOffer>(
            "SELECT `furni_id`, `sprite_id`, `item_id`, `user_id`, `extra_data`, `offer_id`, `state`, `timestamp`, `limited_number`, `limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @offerId LIMIT 1",
            new { offerId });
    }

    public async Task DeleteOffer(int offerId)
    {
        using var connection = _database.Connection();
        await connection.ExecuteAsync("DELETE FROM `catalog_marketplace_offers` WHERE `offer_id` = @offerId LIMIT 1", new { offerId });
    }
}