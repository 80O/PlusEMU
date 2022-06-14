using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Catalog;

public interface ICatalogManager
{
    Dictionary<int, int> ItemOffers { get; }
    void Init(IItemDataManager itemDataManager);
    bool TryGetBot(int itemId, out CatalogBot bot);
    bool TryGetPage(int pageId, out CatalogPage page);
    bool TryGetDeal(int dealId, out CatalogDeal deal);
    ICollection<CatalogPage> GetPages();
    ICollection<CatalogPromotion> GetPromotions();
    IMarketplaceManager GetMarketplace();
    IPetRaceManager GetPetRaceManager();
    IVoucherManager GetVoucherManager();
    IClothingManager GetClothingManager();
}