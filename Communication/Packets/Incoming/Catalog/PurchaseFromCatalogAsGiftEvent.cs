using System.Globalization;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.Utilities;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class PurchaseFromCatalogAsGiftEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IItemDataManager _itemManager;
    private readonly IDatabase _database;
    private readonly IAchievementManager _achievementManager;
    private readonly IGameClientManager _gameClientManager;
    private readonly IQuestManager _questManager;
    private readonly IItemFactory _itemFactory;

    public PurchaseFromCatalogAsGiftEvent(ICatalogManager catalogManager,
        ISettingsManager settingsManager,
        IItemDataManager itemManager,
        IDatabase database,
        IAchievementManager achievementManager,
        IGameClientManager gameClientManager,
        IQuestManager questManager,
        IItemFactory itemFactory)
    {
        _catalogManager = catalogManager;
        _settingsManager = settingsManager;
        _itemManager = itemManager;
        _database = database;
        _achievementManager = achievementManager;
        _gameClientManager = gameClientManager;
        _questManager = questManager;
        _itemFactory = itemFactory;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var pageId = packet.ReadInt();
        var itemId = packet.ReadInt();
        var data = packet.ReadString();
        var giftUser = StringCharFilter.Escape(packet.ReadString());
        var giftMessage = StringCharFilter.Escape(packet.ReadString().Replace(Convert.ToChar(5), ' '));
        var spriteId = packet.ReadInt();
        var boxId = packet.ReadInt();
        var ribbonId = packet.ReadInt();
        packet.ReadBool();
        if (_settingsManager.TryGetValue("room.item.gifts.enabled") != "1")
        {
            session.SendNotification("The hotel managers have disabled gifting");
            return Task.CompletedTask;
        }
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
        if (!ItemUtility.CanGiftItem(item))
            return Task.CompletedTask;
        if (!_itemManager.Gifts.TryGetValue(spriteId, out var presentId) || !_itemManager.Items.TryGetValue(presentId, out var presentData) || presentData.InteractionType != InteractionType.Gift)
            return Task.CompletedTask;
        if (session.GetHabbo().Credits < item.CostCredits)
        {
            session.Send(new PresentDeliverErrorComposer(true, false));
            return Task.CompletedTask;
        }
        if (session.GetHabbo().Duckets < item.CostPixels)
        {
            session.Send(new PresentDeliverErrorComposer(false, true));
            return Task.CompletedTask;
        }
        var habbo = _gameClientManager.GetClientByUsername(giftUser)?.GetHabbo();
        if (habbo == null)
        {
            session.Send(new GiftWrappingErrorComposer());
            return Task.CompletedTask;
        }
        if (!habbo.AllowGifts)
        {
            session.SendNotification("Oops, this user doesn't allow gifts to be sent to them!");
            return Task.CompletedTask;
        }
        if ((DateTime.Now - session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
        {
            session.SendNotification("You're purchasing gifts too fast! Please wait 15 seconds!");
            session.GetHabbo().GiftPurchasingWarnings += 1;
            if (session.GetHabbo().GiftPurchasingWarnings >= 25)
                session.GetHabbo().SessionGiftBlocked = true;
            return Task.CompletedTask;
        }
        if (session.GetHabbo().SessionGiftBlocked)
            return Task.CompletedTask;
        var extra_data = giftUser + Convert.ToChar(5) + giftMessage + Convert.ToChar(5) + session.GetHabbo().Id + Convert.ToChar(5) + item.Definition.Id + Convert.ToChar(5) + spriteId + Convert.ToChar(5) + ribbonId +
                 Convert.ToChar(5) + boxId;
        int newItemId;
        using (var connection = _database.Connection())
        {
            //Insert the dummy item.
            var InsertQuery = connection.Execute("INSERT INTO `items` (`base_item`,`user_id`,`extra_data`) VALUES (@baseId, @habboId, @extra_data)",
                new { baseId = presentData.Id, habboId = habbo.Id, extra_data = extra_data });
            newItemId = Convert.ToInt32(InsertQuery);
            string itemExtraData = null;
            switch (item.Definition.InteractionType)
            {
                case InteractionType.None:
                    itemExtraData = "";
                    break;
                case InteractionType.Pet:
                    try
                    {
                        var bits = data.Split('\n');
                        var petName = bits[0];
                        var race = bits[1];
                        var color = bits[2];
                        if (PetUtility.CheckPetName(petName))
                            return Task.CompletedTask;
                        if (race.Length > 2)
                            return Task.CompletedTask;
                        if (color.Length != 6)
                            return Task.CompletedTask;
                        _achievementManager.ProgressAchievement(session, "ACH_PetLover", 1);
                    }
                    catch
                    {
                        return Task.CompletedTask;
                    }
                    break;
                case InteractionType.Floor:
                case InteractionType.Wallpaper:
                case InteractionType.Landscape:
                    double number = 0;
                    try
                    {
                        number = string.IsNullOrEmpty(data) ? 0 : double.Parse(data, PlusEnvironment.CultureInfo);
                    }
                    catch
                    {
                        //ignored
                    }
                    itemExtraData = number.ToString(CultureInfo.CurrentCulture).Replace(',', '.');
                    break; // maintain extra data // todo: validate
                case InteractionType.Postit:
                    itemExtraData = "FFFF33";
                    break;
                case InteractionType.Moodlight:
                    itemExtraData = "1,1,1,#000000,255";
                    break;
                case InteractionType.Trophy:
                    itemExtraData = $"{session.GetHabbo().Username}{Convert.ToChar(9)}{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}{Convert.ToChar(9)}{data}";
                    break;
                case InteractionType.Mannequin:
                    itemExtraData = $"m{Convert.ToChar(5)}.ch-210-1321.lg-285-92{Convert.ToChar(5)}Default Mannequin";
                    break;
                case InteractionType.BadgeDisplay:
                    if (!session.GetHabbo().Inventory.Badges.HasBadge(data))
                    {
                        session.Send(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                        return Task.CompletedTask;
                    }
                    itemExtraData = $"{data}{Convert.ToChar(9)}{session.GetHabbo().Username}{Convert.ToChar(9)}{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}";
                    break;
                default:
                    itemExtraData = data;
                    break;
            }

            //Insert the present, forever.
            connection.Execute("INSERT INTO `user_presents` (`item_id`,`base_id`,`extra_data`) VALUES (@itemId, @baseId, @extra_data)",
                new {itemId = newItemId, baseId = item.Definition.Id, extra_data = string.IsNullOrEmpty(itemExtraData) ? "" : itemExtraData });

            //Here we're clearing up a record, this is dumb, but okay.
            connection.Execute("DELETE FROM `items` WHERE `id` = @deleteId LIMIT 1", new { deleteId = newItemId});
        }
        var giveItem = _itemFactory.CreateGiftItem(presentData, habbo, extra_data, extra_data, newItemId).ToInventoryItem();
        if (giveItem != null)
        {
            var receiver = _gameClientManager.GetClientByUserId(habbo.Id);
            if (receiver != null)
            {
                receiver.GetHabbo().Inventory.Furniture.AddItem(giveItem);
                receiver.Send(new FurniListNotificationComposer(giveItem.Id, 1));
                receiver.Send(new PurchaseOkComposer());
                receiver.Send(new FurniListAddComposer(giveItem));
                receiver.Send(new FurniListUpdateComposer());
            }

            if (habbo.Id != session.GetHabbo().Id)
            {
                _achievementManager.ProgressAchievement(session, "ACH_GiftGiver", 1);
                if (receiver != null)
                    _achievementManager.ProgressAchievement(receiver, "ACH_GiftReceiver", 1);
                _questManager.ProgressUserQuest(session, QuestType.GiftOthers);
            }
        }
        session.Send(new PurchaseOkComposer(item, presentData));
        if (item.CostCredits > 0)
        {
            session.GetHabbo().Credits -= item.CostCredits;
            session.Send(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        if (item.CostPixels > 0)
        {
            session.GetHabbo().Duckets -= item.CostPixels;
            session.Send(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets));
        }
        session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
        return Task.CompletedTask;
    }
}