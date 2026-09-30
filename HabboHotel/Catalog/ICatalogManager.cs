using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.Catalog.Vouchers;

namespace Plus.HabboHotel.Catalog;

public interface ICatalogManager
{
    Dictionary<int, int> ItemOffers { get; }
    IReadOnlyCollection<CatalogClubOffer> ClubOffers { get; }
    Task Init();
    bool TryGetBot(uint itemId, out CatalogBot bot);
    bool TryGetPage(int pageId, out CatalogPage page);
    bool TryGetDeal(int dealId, out CatalogDeal deal);
    ICollection<CatalogPage> Pages { get; }
    ICollection<CatalogPromotion> Promotions { get; }

    [Obsolete("Use dependency injection instead.")] IMarketplaceManager Marketplace { get; }

    [Obsolete("Use dependency injection instead.")] IPetRaceManager PetRaceManager { get; }

    [Obsolete("Use dependency injection instead.")] IVoucherManager VoucherManager { get; }

    [Obsolete("Use dependency injection instead.")] IClothingManager ClothingManager { get; }
}