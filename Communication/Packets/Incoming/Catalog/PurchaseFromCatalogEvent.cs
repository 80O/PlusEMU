using System.Globalization;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Core;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users.Effects;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class PurchaseFromCatalogEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;
    private readonly IDatabase _database;
    private readonly ISettingsManager _settingsManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IItemDataManager _itemManager;
    private readonly IBadgeManager _badgeManager;
    private readonly IItemFactory _itemFactory;

    public PurchaseFromCatalogEvent(ICatalogManager catalogManager,
        IDatabase database,
        ISettingsManager settingsManager,
        IAchievementManager achievementManager,
        IItemDataManager itemManager,
        IBadgeManager badgeManager,
        IItemFactory itemFactory)
    {
        _catalogManager = catalogManager;
        _database = database;
        _settingsManager = settingsManager;
        _achievementManager = achievementManager;
        _itemManager = itemManager;
        _badgeManager = badgeManager;
        _itemFactory = itemFactory;
    }
    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (_settingsManager.TryGetValue("catalog.enabled") != "1")
        {
            session.SendNotification("The hotel managers have disabled the catalogue");
            return;
        }
        var pageId = packet.ReadInt();
        var itemId = packet.ReadInt();
        var extraData = packet.ReadString();
        var amount = packet.ReadInt();
        if (!_catalogManager.TryGetPage(pageId, out var page))
            return;
        if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            return;
        if (!page.Items.TryGetValue(itemId, out var item))
        {
            if (page.ItemOffers.ContainsKey(itemId))
            {
                item = page.ItemOffers[itemId];
                if (item == null)
                    return;
            }
            else
                return;
        }
        if (amount < 1 || amount > 100 || !item.HaveOffer)
            amount = 1;
        var amountPurchase = item.Amount > 1 ? item.Amount : amount;
        var totalCreditsCost = amount > 1 ? item.CostCredits * amount - (int)Math.Floor((double)amount / 6) * item.CostCredits : item.CostCredits;
        var totalPixelCost = amount > 1 ? item.CostPixels * amount - (int)Math.Floor((double)amount / 6) * item.CostPixels : item.CostPixels;
        var totalDiamondCost = amount > 1 ? item.CostDiamonds * amount - (int)Math.Floor((double)amount / 6) * item.CostDiamonds : item.CostDiamonds;
        if (session.GetHabbo().Credits < totalCreditsCost || session.GetHabbo().Duckets < totalPixelCost || session.GetHabbo().Diamonds < totalDiamondCost)
            return;
        var limitedEditionSells = 0u;
        var limitedEditionStack = 0u;
        switch (item.Definition.InteractionType)
        {
            case InteractionType.None:
                extraData = "";
                break;
            case InteractionType.GuildItem:
            case InteractionType.GuildGate:
                break;
            case InteractionType.Pet:
                try
                {
                    var bits = extraData.Split('\n');
                    var petName = bits[0];
                    var race = bits[1];
                    var color = bits[2];
                    if (!PetUtility.CheckPetName(petName))
                        return;
                    if (race.Length > 2)
                        return;
                    if (color.Length != 6)
                        return;
                    _achievementManager.ProgressAchievement(session, "ACH_PetLover", 1);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                    return;
                }
                break;
            case InteractionType.Floor:
            case InteractionType.Wallpaper:
            case InteractionType.Landscape:
                double number = 0;
                try
                {
                    number = string.IsNullOrEmpty(extraData) ? 0 : double.Parse(extraData, PlusEnvironment.CultureInfo);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
                extraData = number.ToString(CultureInfo.CurrentCulture).Replace(',', '.');
                break; // maintain extra data // todo: validate
            case InteractionType.Postit:
                extraData = "FFFF33";
                break;
            case InteractionType.Moodlight:
                extraData = "1,1,1,#000000,255";
                break;
            case InteractionType.Trophy:
                extraData = $"{session.GetHabbo().Username}{Convert.ToChar(9)}{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}{Convert.ToChar(9)}{extraData}";
                break;
            case InteractionType.Mannequin:
                extraData = $"m{Convert.ToChar(5)}.ch-210-1321.lg-285-92{Convert.ToChar(5)}Default Mannequin";
                break;
            case InteractionType.BadgeDisplay:
                if (!session.GetHabbo().Inventory.Badges.HasBadge(extraData))
                {
                    session.Send(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                    return;
                }
                extraData = $"{extraData}{Convert.ToChar(9)}{session.GetHabbo().Username}{Convert.ToChar(9)}{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}";
                break;
            case InteractionType.Badge:
            {
                if (session.GetHabbo().Inventory.Badges.HasBadge(item.Definition.ItemName))
                {
                    session.Send(new PurchaseErrorComposer(1));
                    return;
                }
                break;
            }
            default:
                extraData = "";
                break;
        }
        if (item.IsLimited)
        {
            if (item.LimitedEditionStack <= item.LimitedEditionSells)
            {
                session.SendNotification("This item has sold out!\n\n" + "Please note, you have not recieved another item (You have also not been charged for it!)");
                session.Send(new CatalogUpdatedComposer());
                session.Send(new PurchaseOkComposer());
                return;
            }
            item.LimitedEditionSells++;
            using var connection = _database.Connection();
            connection.Execute("UPDATE `catalog_items` SET `limited_sells` = @limitedSells WHERE `id` = @itemId LIMIT 1",
                new { limitedSells = item.LimitedEditionSells, itemId = item.Id });

            limitedEditionSells = item.LimitedEditionSells;
            limitedEditionStack = item.LimitedEditionStack;
        }
        if (item.CostCredits > 0)
        {
            session.GetHabbo().AdjustCredits(-(totalCreditsCost));
            session.Send(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        if (item.CostPixels > 0)
        {
            session.GetHabbo().Duckets -= totalPixelCost;
            session.Send(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets)); //Love you, Tom.
        }
        if (item.CostDiamonds > 0)
        {
            session.GetHabbo().Diamonds -= totalDiamondCost;
            session.Send(new HabboActivityPointNotificationComposer(session.GetHabbo().Diamonds, 0, 5));
        }
        switch (item.Definition.Type.ToString().ToLower())
        {
            default:
                var generatedGenericItems = new List<Item>();
                Item newItem;
                switch (item.Definition.InteractionType)
                {
                    default:
                        if (amountPurchase > 1)
                        {
                            var items = _itemFactory.CreateMultipleItems(item.Definition, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null) generatedGenericItems.AddRange(items);
                        }
                        else
                        {
                            newItem = _itemFactory.CreateSingleItemNullable(item.Definition, session.GetHabbo(), extraData, extraData, 0, limitedEditionSells, limitedEditionStack);
                            if (newItem != null) generatedGenericItems.Add(newItem);
                        }
                        break;
                    case InteractionType.GuildGate:
                    case InteractionType.GuildItem:
                    case InteractionType.GuildForum:
                        if (amountPurchase > 1)
                        {
                            var items = _itemFactory.CreateMultipleItems(item.Definition, session.GetHabbo(), extraData, amountPurchase, Convert.ToInt32(extraData));
                            if (items != null) generatedGenericItems.AddRange(items);
                        }
                        else
                        {
                            newItem = _itemFactory.CreateSingleItemNullable(item.Definition, session.GetHabbo(), extraData, extraData, Convert.ToInt32(extraData));
                            if (newItem != null) generatedGenericItems.Add(newItem);
                        }
                        break;
                    case InteractionType.Arrow:
                    case InteractionType.Teleport:
                        for (var i = 0; i < amountPurchase; i++)
                        {
                            var teleItems = _itemFactory.CreateTeleporterItems(item.Definition, session.GetHabbo());
                            if (teleItems != null) generatedGenericItems.AddRange(teleItems);
                        }
                        break;
                    case InteractionType.Moodlight:
                    {
                        if (amountPurchase > 1)
                        {
                            var items = _itemFactory.CreateMultipleItems(item.Definition, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                                foreach (var I in items) _itemFactory.CreateMoodlightData(I);
                            }
                        }
                        else
                        {
                            newItem = _itemFactory.CreateSingleItemNullable(item.Definition, session.GetHabbo(), extraData, extraData);
                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                                _itemFactory.CreateMoodlightData(newItem);
                            }
                        }
                    }
                        break;
                    case InteractionType.Toner:
                    {
                        if (amountPurchase > 1)
                        {
                            var items = _itemFactory.CreateMultipleItems(item.Definition, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                                foreach (var I in items) _itemFactory.CreateTonerData(I);
                            }
                        }
                        else
                        {
                            newItem = _itemFactory.CreateSingleItemNullable(item.Definition, session.GetHabbo(), extraData, extraData);
                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                                _itemFactory.CreateTonerData(newItem);
                            }
                        }
                    }
                        break;
                    case InteractionType.Deal:
                    {
                        if (_catalogManager.TryGetDeal(item.Definition.BehaviourData, out var deal))
                        {
                            foreach (var catalogItem in deal.ItemDataList.ToList())
                            {
                                var items = _itemFactory.CreateMultipleItems(catalogItem.Definition, session.GetHabbo(), "", amountPurchase);
                                if (items != null) generatedGenericItems.AddRange(items);
                            }
                        }
                        break;
                    }
                }
                foreach (var purchasedItem in generatedGenericItems)
                {
                    if (session.GetHabbo().Inventory.Furniture.AddItem(purchasedItem.ToInventoryItem()))
                    {
                        //Session.SendMessage(new FurniListAddComposer(PurchasedItem));
                        session.Send(new FurniListNotificationComposer(purchasedItem.Id, 1));
                    }
                }
                break;
            case "e":
                AvatarEffect effect;
                if (session.GetHabbo().Effects.HasEffect(item.Definition.SpriteId))
                {
                    effect = session.GetHabbo().Effects.GetEffectNullable(item.Definition.SpriteId);
                    if (effect != null) effect.AddToQuantity();
                }
                else
                    effect = AvatarEffectFactory.CreateNullable(session.GetHabbo(), item.Definition.SpriteId, 3600);
                if (effect != null) // && Session.GetHabbo().Effects().TryAdd(Effect))
                    session.Send(new AvatarEffectAddedComposer(item.Definition.SpriteId, 3600));
                break;
            case "r":
                var bot = BotUtility.CreateBot(item.Definition, session.GetHabbo().Id);
                if (bot != null)
                {
                    session.GetHabbo().Inventory.Bots.AddBot(bot);
                    session.Send(new BotInventoryComposer(session.GetHabbo().Inventory.Bots.Bots.Values.ToList()));
                    session.Send(new FurniListNotificationComposer((uint)bot.Id, 5));
                }
                else
                    session.SendNotification("Oops! There was an error whilst purchasing this bot. It seems that there is no bot data for the bot!");
                break;
            case "b":
            {
                await _badgeManager.GiveBadge(session.GetHabbo(), item.Definition.ItemName);
                session.Send(new FurniListNotificationComposer(0, 4));
                break;
            }
            case "p":
            {
                var petData = extraData.Split('\n');
                var pet = PetUtility.CreatePet(session.GetHabbo().Id, petData[0], item.Definition.BehaviourData, petData[1], petData[2]);
                if (pet != null)
                {
                    if (session.GetHabbo().Inventory.Pets.AddPet(pet))
                    {
                        pet.RoomId = 0;
                        pet.PlacedInRoom = false;
                        session.Send(new FurniListNotificationComposer((uint)pet.PetId, 3));
                        session.Send(new PetInventoryComposer(session.GetHabbo().Inventory.Pets.Pets.Values.ToList()));
                        if (_itemManager.Items.TryGetValue(320, out var petFood))
                        {
                            var food = _itemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "", "").ToInventoryItem();
                            if (food != null)
                            {
                                session.GetHabbo().Inventory.Furniture.AddItem(food);
                                session.Send(new FurniListNotificationComposer(food.Id, 1));
                            }
                        }
                    }
                }
                break;
            }
        }
        if (!string.IsNullOrEmpty(item.Badge) &&
            _badgeManager.Badges.TryGetValue(item.Badge, out var badge) &&
            (string.IsNullOrEmpty(badge.RequiredRight) || session.GetHabbo().Permissions.HasRight(badge.RequiredRight)))
            await _badgeManager.GiveBadge(session.GetHabbo(), badge.Code);
        session.Send(new PurchaseOkComposer(item, item.Definition));
        session.Send(new FurniListUpdateComposer());
    }
}
