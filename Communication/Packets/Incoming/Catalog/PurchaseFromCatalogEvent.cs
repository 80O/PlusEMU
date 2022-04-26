using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Users.Effects;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class PurchaseFromCatalogEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;
    private readonly IDatabase _database;
    private readonly ISettingsManager _settingsManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;
    private readonly IGameClientManager _gameClientManager;
    private readonly IItemDataManager _itemManager;
    private readonly IBadgeManager _badgeManager;

    public PurchaseFromCatalogEvent(ICatalogManager catalogManager,
        IDatabase database,
        ISettingsManager settingsManager,
        IAchievementManager achievementManager,
        IQuestManager questManager,
        IGameClientManager gameClientManager,
        IItemDataManager itemManager,
        IBadgeManager badgeManager)
    {
        _catalogManager = catalogManager;
        _database = database;
        _settingsManager = settingsManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
        _gameClientManager = gameClientManager;
        _itemManager = itemManager;
        _badgeManager = badgeManager;
    }
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (_settingsManager.TryGetValue("catalog.enabled") != "1")
        {
            session.SendNotification("The hotel managers have disabled the catalogue");
            return Task.CompletedTask;
        }
        var pageId = packet.PopInt();
        var itemId = packet.PopInt();
        var extraData = packet.PopString();
        var amount = packet.PopInt();
        if (!_catalogManager.TryGetPage(pageId, out var page))
            return Task.CompletedTask;
        if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            return Task.CompletedTask;
        if (!page.Items.TryGetValue(itemId, out var item))
        {
            if (page.ItemOffers.ContainsKey(itemId))
            {
                item = page.ItemOffers[itemId];
                if (item == null)
                    return Task.CompletedTask;
            }
            else
                return Task.CompletedTask;
        }
        if (amount < 1 || amount > 100 || !item.HaveOffer)
            amount = 1;
        var amountPurchase = item.Amount > 1 ? item.Amount : amount;
        var totalCreditsCost = amount > 1 ? item.CostCredits * amount - (int)Math.Floor((double)amount / 6) * item.CostCredits : item.CostCredits;
        var totalPixelCost = amount > 1 ? item.CostPixels * amount - (int)Math.Floor((double)amount / 6) * item.CostPixels : item.CostPixels;
        var totalDiamondCost = amount > 1 ? item.CostDiamonds * amount - (int)Math.Floor((double)amount / 6) * item.CostDiamonds : item.CostDiamonds;
        if (session.GetHabbo().Credits < totalCreditsCost || session.GetHabbo().Duckets < totalPixelCost || session.GetHabbo().Diamonds < totalDiamondCost)
            return Task.CompletedTask;
        var limitedEditionSells = 0;
        var limitedEditionStack = 0;
        switch (item.Data.InteractionType)
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
                        return Task.CompletedTask;
                    if (race.Length > 2)
                        return Task.CompletedTask;
                    if (color.Length != 6)
                        return Task.CompletedTask;
                    _achievementManager.ProgressAchievement(session, "ACH_PetLover", 1);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                    return Task.CompletedTask;
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
                extraData = session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + extraData;
                break;
            case InteractionType.Mannequin:
                extraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
                break;
            case InteractionType.BadgeDisplay:
                if (!session.GetHabbo().GetBadgeComponent().HasBadge(extraData))
                {
                    session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                    return Task.CompletedTask;
                }
                extraData = extraData + Convert.ToChar(9) + session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
                break;
            case InteractionType.Badge:
            {
                if (session.GetHabbo().GetBadgeComponent().HasBadge(item.Data.ItemName))
                {
                    session.SendPacket(new PurchaseErrorComposer(1));
                    return Task.CompletedTask;
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
                session.SendPacket(new CatalogUpdatedComposer());
                session.SendPacket(new PurchaseOkComposer());
                return Task.CompletedTask;
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
            session.GetHabbo().Credits -= totalCreditsCost;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        if (item.CostPixels > 0)
        {
            session.GetHabbo().Duckets -= totalPixelCost;
            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets)); //Love you, Tom.
        }
        if (item.CostDiamonds > 0)
        {
            session.GetHabbo().Diamonds -= totalDiamondCost;
            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Diamonds, 0, 5));
        }
        switch (item.Data.Type.ToString().ToLower())
        {
            default:
                var generatedGenericItems = new List<Item>();
                Item newItem;
                switch (item.Data.InteractionType)
                {
                    default:
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null) generatedGenericItems.AddRange(items);
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, extraData, 0, limitedEditionSells, limitedEditionStack);
                            if (newItem != null) generatedGenericItems.Add(newItem);
                        }
                        break;
                    case InteractionType.GuildGate:
                    case InteractionType.GuildItem:
                    case InteractionType.GuildForum:
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase, Convert.ToInt32(extraData));
                            if (items != null) generatedGenericItems.AddRange(items);
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, extraData, Convert.ToInt32(extraData));
                            if (newItem != null) generatedGenericItems.Add(newItem);
                        }
                        break;
                    case InteractionType.Arrow:
                    case InteractionType.Teleport:
                        for (var i = 0; i < amountPurchase; i++)
                        {
                            var teleItems = ItemFactory.CreateTeleporterItems(item.Data, session.GetHabbo());
                            if (teleItems != null) generatedGenericItems.AddRange(teleItems);
                        }
                        break;
                    case InteractionType.Moodlight:
                    {
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                                foreach (var I in items) ItemFactory.CreateMoodlightData(I);
                            }
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, extraData);
                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                                ItemFactory.CreateMoodlightData(newItem);
                            }
                        }
                    }
                        break;
                    case InteractionType.Toner:
                    {
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amountPurchase);
                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                                foreach (var I in items) ItemFactory.CreateTonerData(I);
                            }
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, extraData);
                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                                ItemFactory.CreateTonerData(newItem);
                            }
                        }
                    }
                        break;
                    case InteractionType.Deal:
                    {
                        if (_catalogManager.TryGetDeal(item.Data.BehaviourData, out var deal))
                        {
                            foreach (var catalogItem in deal.ItemDataList.ToList())
                            {
                                var items = ItemFactory.CreateMultipleItems(catalogItem.Data, session.GetHabbo(), "", amountPurchase);
                                if (items != null) generatedGenericItems.AddRange(items);
                            }
                        }
                        break;
                    }
                }
                foreach (var purchasedItem in generatedGenericItems)
                {
                    if (session.GetHabbo().GetInventoryComponent().TryAddItem(purchasedItem))
                    {
                        //Session.SendMessage(new FurniListAddComposer(PurchasedItem));
                        session.SendPacket(new FurniListNotificationComposer(purchasedItem.Id, 1));
                    }
                }
                break;
            case "e":
                AvatarEffect effect;
                if (session.GetHabbo().Effects().HasEffect(item.Data.SpriteId))
                {
                    effect = session.GetHabbo().Effects().GetEffectNullable(item.Data.SpriteId);
                    if (effect != null) effect.AddToQuantity();
                }
                else
                    effect = AvatarEffectFactory.CreateNullable(session.GetHabbo(), item.Data.SpriteId, 3600);
                if (effect != null) // && Session.GetHabbo().Effects().TryAdd(Effect))
                    session.SendPacket(new AvatarEffectAddedComposer(item.Data.SpriteId, 3600));
                break;
            case "r":
                var bot = BotUtility.CreateBot(item.Data, session.GetHabbo().Id);
                if (bot != null)
                {
                    session.GetHabbo().GetInventoryComponent().TryAddBot(bot);
                    session.SendPacket(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
                    session.SendPacket(new FurniListNotificationComposer(bot.Id, 5));
                }
                else
                    session.SendNotification("Oops! There was an error whilst purchasing this bot. It seems that there is no bot data for the bot!");
                break;
            case "b":
            {
                session.GetHabbo().GetBadgeComponent().GiveBadge(item.Data.ItemName, true, session);
                session.SendPacket(new FurniListNotificationComposer(0, 4));
                break;
            }
            case "p":
            {
                var petData = extraData.Split('\n');
                var pet = PetUtility.CreatePet(session.GetHabbo().Id, petData[0], item.Data.BehaviourData, petData[1], petData[2]);
                if (pet != null)
                {
                    if (session.GetHabbo().GetInventoryComponent().TryAddPet(pet))
                    {
                        pet.RoomId = 0;
                        pet.PlacedInRoom = false;
                        session.SendPacket(new FurniListNotificationComposer(pet.PetId, 3));
                        session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
                        if (_itemManager.GetItem(320, out var petFood))
                        {
                            var food = ItemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "", "");
                            if (food != null)
                            {
                                session.GetHabbo().GetInventoryComponent().TryAddItem(food);
                                session.SendPacket(new FurniListNotificationComposer(food.Id, 1));
                            }
                        }
                    }
                }
                break;
            }
        }
        if (!string.IsNullOrEmpty(item.Badge) &&
            _badgeManager.TryGetBadge(item.Badge, out var badge) &&
            (string.IsNullOrEmpty(badge.RequiredRight) || session.GetHabbo().GetPermissions().HasRight(badge.RequiredRight)))
            session.GetHabbo().GetBadgeComponent().GiveBadge(badge.Code, true, session);
        session.SendPacket(new PurchaseOkComposer(item, item.Data));
        session.SendPacket(new FurniListUpdateComposer());
        return Task.CompletedTask;
    }
}