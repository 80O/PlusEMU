﻿using System.Buffers.Text;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;
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
    private readonly Dictionary<uint, CatalogBot> _botPresets;
    private readonly Dictionary<int, CatalogDeal> _deals;
    private readonly Dictionary<int, Dictionary<int, CatalogItem>> _items;
    private readonly Dictionary<int, CatalogPage> _pages;
    private readonly Dictionary<int, CatalogPromotion> _promotions;
    private Dictionary<int, int> _itemOffers;

    private readonly IClothingManager _clothingManager;
    private readonly IDatabase _database;
    private readonly IMarketplaceManager _marketplace;
    private readonly IPetRaceManager _petRaceManager;
    private readonly IVoucherManager _voucherManager;
    private readonly IItemDataManager _itemDataManager;

    public CatalogManager(IMarketplaceManager marketplace, IPetRaceManager petRaceManager, IVoucherManager voucherManager, IClothingManager clothingManager, IDatabase database, ILogger<CatalogManager> logger, IItemDataManager itemDataManager)
    {
        _marketplace = marketplace;
        _petRaceManager = petRaceManager;
        _voucherManager = voucherManager;
        _clothingManager = clothingManager;
        _itemDataManager = itemDataManager;
        _database = database;
        _logger = logger;
        _itemOffers = new();
        _pages = new();
        _botPresets = new();
        _items = new();
        _deals = new();
        _promotions = new();
    }

    public Dictionary<int, int> ItemOffers => _itemOffers;

    public async Task Init()
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

        using var connection = _database.Connection();

        var items = await connection.QueryAsync<CatalogItem>("SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
        foreach(CatalogItem item in items)
        {
            if (item.Amount <= 0)
                continue;

            if (!_itemDataManager.Items.TryGetValue(item.ItemId, out ItemDefinition? definition))
            {
                _logger.LogError("Couldn't load Catalog Item " + item.ItemId + ", no furniture record found.");
                continue;
            }

            if (!_items.ContainsKey(item.PageId))
                _items[item.PageId] = new();

            if (item.OfferId != -1 && !_itemOffers.ContainsKey(item.OfferId))
                _itemOffers.Add(item.OfferId, item.PageId);

            item.Definition = definition;
            _items[item.PageId].Add(item.Id, item);
        }

        var deals = await connection.QueryAsync<CatalogDeal>("SELECT `id`, `items`, `name`, `room_id` FROM `catalog_deals`");
        foreach (CatalogDeal deal in deals)
        {
            if (_deals.ContainsKey(deal.Id))
                continue;

            var itemDataList = new List<CatalogItem>();
            if (!string.IsNullOrWhiteSpace(deal.Items))
            {
                var splitItems = deal.Items.Split(';');
                foreach (var split in splitItems)
                {
                    var item = split.Split('*');
                    if (!uint.TryParse(item[0], out var itemId) || !int.TryParse(item[1], out var amount))
                        continue;

                    if (!_itemDataManager.Items.TryGetValue(itemId, out var data))
                        continue;

                    itemDataList.Add(new()
                    {
                        Id = 0,
                        ItemId = itemId,
                        Definition = data,
                        CatalogName = string.Empty,
                        PageId = 0,
                        CostCredits = 0,
                        CostPixels = 0,
                        CostDiamonds = 0,
                        Amount = amount,
                        LimitedEditionSells = 0,
                        LimitedEditionStack = 0,
                        HaveOffer = true,
                        ExtraData = "",
                        Badge = "",
                        OfferId = 0
                    });
                }
                deal.ItemDataList = itemDataList;
            }

            _deals.Add(deal.Id, deal);
        }

        var pages = await connection.QueryAsync<CatalogPage>("SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
        foreach (CatalogPage page in pages)
        {
            if (_items.ContainsKey(page.Id))
                page.Items = _items[page.Id];

            page.PageStringsList1 = !string.IsNullOrWhiteSpace(page.PageStrings1) ? page.PageStrings1!.Split("|").ToList() : new();
            page.PageStringsList2 = !string.IsNullOrWhiteSpace(page.PageStrings2) ? page.PageStrings2!.Split("|").ToList() : new();
            _pages.Add(page.Id, page);
        }

        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
            var bots = dbClient.GetTable();
            if (bots != null)
            {
                foreach (DataRow row in bots.Rows)
                {
                    _botPresets.Add(Convert.ToUInt32(row[0]),
                        new(Convert.ToInt32(row[0]), Convert.ToString(row[1]), Convert.ToString(row[2]), Convert.ToString(row[3]), Convert.ToString(row[4]), Convert.ToString(row[5])));
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
                            new(Convert.ToInt32(row["id"]), Convert.ToString(row["title"]), Convert.ToString(row["image"]), Convert.ToInt32(row["unknown"]),
                                Convert.ToString(row["page_link"]), Convert.ToInt32(row["parent_id"])));
                    }
                }
            }
            _petRaceManager.Init();
            _clothingManager.Init();
        }
        _logger.LogInformation("Catalog Manager -> LOADED");
    }

    public bool TryGetBot(uint itemId, out CatalogBot bot) => _botPresets.TryGetValue(itemId, out bot);

    public bool TryGetPage(int pageId, out CatalogPage page) => _pages.TryGetValue(pageId, out page);

    public bool TryGetDeal(int dealId, out CatalogDeal deal) => _deals.TryGetValue(dealId, out deal);

    public ICollection<CatalogPage> Pages => _pages.Values;

    public ICollection<CatalogPromotion> Promotions => _promotions.Values;

    public IMarketplaceManager Marketplace => _marketplace;

    public IPetRaceManager PetRaceManager => _petRaceManager;

    public IVoucherManager VoucherManager => _voucherManager;

    public IClothingManager ClothingManager => _clothingManager;
}