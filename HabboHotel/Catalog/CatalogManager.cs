using System.Data;
using Microsoft.Extensions.Logging;
using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.Items;
using Plus.Utilities;

namespace Plus.HabboHotel.Catalog;

public class CatalogManager : ICatalogManager
{
    private readonly ILogger<CatalogManager> _logger;
    private readonly Dictionary<int, CatalogBot> _botPresets;
    private readonly Dictionary<int, CatalogDeal> _deals;
    private readonly Dictionary<int, Dictionary<int, CatalogItem>> _items;
    private readonly Dictionary<int, CatalogPage> _pages;
    private readonly Dictionary<int, CatalogPromotion> _promotions;
    private Dictionary<int, int> _itemOffers;

    private readonly IClothingManager _clothingManager;
    private readonly IMarketplaceManager _marketplace;
    private readonly IPetRaceManager _petRaceManager;
    private readonly IVoucherManager _voucherManager;

    public CatalogManager(IMarketplaceManager marketplace, IPetRaceManager petRaceManager, IVoucherManager voucherManager, IClothingManager clothingManager, ILogger<CatalogManager> logger)
    {
        _marketplace = marketplace;
        _petRaceManager = petRaceManager;
        _voucherManager = voucherManager;
        _clothingManager = clothingManager;
        _logger = logger;
        _itemOffers = new Dictionary<int, int>();
        _pages = new Dictionary<int, CatalogPage>();
        _botPresets = new Dictionary<int, CatalogBot>();
        _items = new Dictionary<int, Dictionary<int, CatalogItem>>();
        _deals = new Dictionary<int, CatalogDeal>();
        _promotions = new Dictionary<int, CatalogPromotion>();
    }

    public Dictionary<int, int> ItemOffers => _itemOffers;

    public void Init(IItemDataManager itemDataManager)
    {
        _voucherManager.Init();
        _clothingManager.Init();
        if (_pages.Count > 0)
            _pages.Clear();
        if (_botPresets.Count > 0)
            _botPresets.Clear();
        if (_items.Count > 0)
            _items.Clear();
        if (_deals.Count > 0)
            _deals.Clear();
        if (_promotions.Count > 0)
            _promotions.Clear();
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery(
                "SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
            var catalogueItems = dbClient.GetTable();
            if (catalogueItems != null)
            {
                foreach (DataRow row in catalogueItems.Rows)
                {
                    if (Convert.ToInt32(row["amount"]) <= 0)
                        continue;
                    var itemId = Convert.ToInt32(row["id"]);
                    var pageId = Convert.ToInt32(row["page_id"]);
                    var baseId = Convert.ToInt32(row["item_id"]);
                    var offerId = Convert.ToInt32(row["offer_id"]);
                    if (!itemDataManager.GetItem(baseId, out var data))
                    {
                        _logger.LogError("Couldn't load Catalog Item " + itemId + ", no furniture record found.");
                        continue;
                    }
                    if (!_items.ContainsKey(pageId))
                        _items[pageId] = new Dictionary<int, CatalogItem>();
                    if (offerId != -1 && !_itemOffers.ContainsKey(offerId))
                        _itemOffers.Add(offerId, pageId);
                    _items[pageId].Add(Convert.ToInt32(row["id"]), new CatalogItem(Convert.ToInt32(row["id"]), Convert.ToInt32(row["item_id"]),
                        data, Convert.ToString(row["catalog_name"]), Convert.ToInt32(row["page_id"]), Convert.ToInt32(row["cost_credits"]), Convert.ToInt32(row["cost_pixels"]),
                        Convert.ToInt32(row["cost_diamonds"]),
                        Convert.ToInt32(row["amount"]), Convert.ToInt32(row["limited_sells"]), Convert.ToInt32(row["limited_stack"]), ConvertExtensions.EnumToBool(row["offer_active"].ToString()),
                        Convert.ToString(row["extradata"]), Convert.ToString(row["badge"]), Convert.ToInt32(row["offer_id"])));
                }
            }
            dbClient.SetQuery("SELECT `id`, `items`, `name`, `room_id` FROM `catalog_deals`");
            var getDeals = dbClient.GetTable();
            if (getDeals != null)
            {
                foreach (DataRow row in getDeals.Rows)
                {
                    var id = Convert.ToInt32(row["id"]);
                    var items = Convert.ToString(row["items"]);
                    var name = Convert.ToString(row["name"]);
                    var roomId = Convert.ToInt32(row["room_id"]);
                    var deal = new CatalogDeal(id, items, name, roomId, itemDataManager);
                    if (!_deals.ContainsKey(id))
                        _deals.Add(deal.Id, deal);
                }
            }
            dbClient.SetQuery(
                "SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
            var catalogPages = dbClient.GetTable();
            if (catalogPages != null)
            {
                foreach (DataRow row in catalogPages.Rows)
                {
                    _pages.Add(Convert.ToInt32(row["id"]), new CatalogPage(Convert.ToInt32(row["id"]), Convert.ToInt32(row["parent_id"]), row["enabled"].ToString(), Convert.ToString(row["caption"]),
                        Convert.ToString(row["page_link"]), Convert.ToInt32(row["icon_image"]), Convert.ToInt32(row["min_rank"]), Convert.ToInt32(row["min_vip"]), row["visible"].ToString(),
                        Convert.ToString(row["page_layout"]),
                        Convert.ToString(row["page_strings_1"]), Convert.ToString(row["page_strings_2"]),
                        _items.ContainsKey(Convert.ToInt32(row["id"])) ? _items[Convert.ToInt32(row["id"])] : new Dictionary<int, CatalogItem>(), ref _itemOffers));
                }
            }
            dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
            var bots = dbClient.GetTable();
            if (bots != null)
            {
                foreach (DataRow row in bots.Rows)
                {
                    _botPresets.Add(Convert.ToInt32(row[0]),
                        new CatalogBot(Convert.ToInt32(row[0]), Convert.ToString(row[1]), Convert.ToString(row[2]), Convert.ToString(row[3]), Convert.ToString(row[4]), Convert.ToString(row[5])));
                }
            }
            dbClient.SetQuery("SELECT * FROM `catalog_promotions`");
            var getPromotions = dbClient.GetTable();
            if (getPromotions != null)
            {
                foreach (DataRow row in getPromotions.Rows)
                {
                    if (!_promotions.ContainsKey(Convert.ToInt32(row["id"])))
                    {
                        _promotions.Add(Convert.ToInt32(row["id"]),
                            new CatalogPromotion(Convert.ToInt32(row["id"]), Convert.ToString(row["title"]), Convert.ToString(row["image"]), Convert.ToInt32(row["unknown"]),
                                Convert.ToString(row["page_link"]), Convert.ToInt32(row["parent_id"])));
                    }
                }
            }
            _petRaceManager.Init();
            _clothingManager.Init();
        }
        _logger.LogInformation("Catalog Manager -> LOADED");
    }

    public bool TryGetBot(int itemId, out CatalogBot bot) => _botPresets.TryGetValue(itemId, out bot);

    public bool TryGetPage(int pageId, out CatalogPage page) => _pages.TryGetValue(pageId, out page);

    public bool TryGetDeal(int dealId, out CatalogDeal deal) => _deals.TryGetValue(dealId, out deal);

    public ICollection<CatalogPage> GetPages() => _pages.Values;

    public ICollection<CatalogPromotion> GetPromotions() => _promotions.Values;

    public IMarketplaceManager GetMarketplace() => _marketplace;

    public IPetRaceManager GetPetRaceManager() => _petRaceManager;

    public IVoucherManager GetVoucherManager() => _voucherManager;

    public IClothingManager GetClothingManager() => _clothingManager;
}