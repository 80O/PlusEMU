using System;
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

    public PurchaseFromCatalogAsGiftEvent(ICatalogManager catalogManager,
        ISettingsManager settingsManager,
        IItemDataManager itemManager,
        IDatabase database,
        IAchievementManager achievementManager,
        IGameClientManager gameClientManager,
        IQuestManager questManager)
    {
        _catalogManager = catalogManager;
        _settingsManager = settingsManager;
        _itemManager = itemManager;
        _database = database;
        _achievementManager = achievementManager;
        _gameClientManager = gameClientManager;
        _questManager = questManager;
    }
    public void Parse(GameClient session, ClientPacket packet)
    {
        var pageId = packet.PopInt();
        var itemId = packet.PopInt();
        var data = packet.PopString();
        var giftUser = StringCharFilter.Escape(packet.PopString());
        var giftMessage = StringCharFilter.Escape(packet.PopString().Replace(Convert.ToChar(5), ' '));
        var spriteId = packet.PopInt();
        var ribbon = packet.PopInt();
        var colour = packet.PopInt();
        packet.PopBoolean();
        if (_settingsManager.TryGetValue("room.item.gifts.enabled") != "1")
        {
            session.SendNotification("The hotel managers have disabled gifting");
            return;
        }
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
        if (!ItemUtility.CanGiftItem(item))
            return;
        if (!_itemManager.GetGift(spriteId, out var presentData) || presentData.InteractionType != InteractionType.Gift)
            return;
        if (session.GetHabbo().Credits < item.CostCredits)
        {
            session.SendPacket(new PresentDeliverErrorMessageComposer(true, false));
            return;
        }
        if (session.GetHabbo().Duckets < item.CostPixels)
        {
            session.SendPacket(new PresentDeliverErrorMessageComposer(false, true));
            return;
        }
        var habbo = PlusEnvironment.GetHabboByUsername(giftUser);
        if (habbo == null)
        {
            session.SendPacket(new GiftWrappingErrorComposer());
            return;
        }
        if (!habbo.AllowGifts)
        {
            session.SendNotification("Oops, this user doesn't allow gifts to be sent to them!");
            return;
        }
        if ((DateTime.Now - session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
        {
            session.SendNotification("You're purchasing gifts too fast! Please wait 15 seconds!");
            session.GetHabbo().GiftPurchasingWarnings += 1;
            if (session.GetHabbo().GiftPurchasingWarnings >= 25)
                session.GetHabbo().SessionGiftBlocked = true;
            return;
        }
        if (session.GetHabbo().SessionGiftBlocked)
            return;
        var ed = giftUser + Convert.ToChar(5) + giftMessage + Convert.ToChar(5) + session.GetHabbo().Id + Convert.ToChar(5) + item.Data.Id + Convert.ToChar(5) + spriteId + Convert.ToChar(5) + ribbon +
                 Convert.ToChar(5) + colour;
        int newItemId;
        using (var dbClient = _database.GetQueryReactor())
        {
            //Insert the dummy item.
            dbClient.SetQuery("INSERT INTO `items` (`base_item`,`user_id`,`extra_data`) VALUES (@baseId, @habboId, @extra_data)");
            dbClient.AddParameter("baseId", presentData.Id);
            dbClient.AddParameter("habboId", habbo.Id);
            dbClient.AddParameter("extra_data", ed);
            newItemId = Convert.ToInt32(dbClient.InsertQuery());
            string itemExtraData = null;
            switch (item.Data.InteractionType)
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
                            return;
                        if (race.Length > 2)
                            return;
                        if (color.Length != 6)
                            return;
                        _achievementManager.ProgressAchievement(session, "ACH_PetLover", 1);
                    }
                    catch
                    {
                        return;
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
                    itemExtraData = session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + data;
                    break;
                case InteractionType.Mannequin:
                    itemExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";
                    break;
                case InteractionType.BadgeDisplay:
                    if (!session.GetHabbo().GetBadgeComponent().HasBadge(data))
                    {
                        session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                        return;
                    }
                    itemExtraData = data + Convert.ToChar(9) + session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
                    break;
                default:
                    itemExtraData = data;
                    break;
            }

            //Insert the present, forever.
            dbClient.SetQuery("INSERT INTO `user_presents` (`item_id`,`base_id`,`extra_data`) VALUES (@itemId, @baseId, @extra_data)");
            dbClient.AddParameter("itemId", newItemId);
            dbClient.AddParameter("baseId", item.Data.Id);
            dbClient.AddParameter("extra_data", string.IsNullOrEmpty(itemExtraData) ? "" : itemExtraData);
            dbClient.RunQuery();

            //Here we're clearing up a record, this is dumb, but okay.
            dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @deleteId LIMIT 1");
            dbClient.AddParameter("deleteId", newItemId);
            dbClient.RunQuery();
        }
        var giveItem = ItemFactory.CreateGiftItem(presentData, habbo, ed, ed, newItemId);
        if (giveItem != null)
        {
            var receiver = _gameClientManager.GetClientByUserId(habbo.Id);
            if (receiver != null)
            {
                receiver.GetHabbo().GetInventoryComponent().TryAddItem(giveItem);
                receiver.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
                receiver.SendPacket(new PurchaseOkComposer());
                receiver.SendPacket(new FurniListAddComposer(giveItem));
                receiver.SendPacket(new FurniListUpdateComposer());
            }

            if (habbo.Id != session.GetHabbo().Id /*&& !string.IsNullOrWhiteSpace(giftMessage)*/)
            {
                _achievementManager.ProgressAchievement(session, "ACH_GiftGiver", 1);
                if (receiver != null)
                    _achievementManager.ProgressAchievement(receiver, "ACH_GiftReceiver", 1);
                _questManager.ProgressUserQuest(session, QuestType.GiftOthers);
            }
        }
        session.SendPacket(new PurchaseOkComposer(item, presentData));
        if (item.CostCredits > 0)
        {
            session.GetHabbo().Credits -= item.CostCredits;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        if (item.CostPixels > 0)
        {
            session.GetHabbo().Duckets -= item.CostPixels;
            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, session.GetHabbo().Duckets));
        }
        session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
    }
}