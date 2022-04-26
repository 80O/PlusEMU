using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.Database;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class OpenGiftEvent : IPacketEvent
{
    private readonly IItemDataManager _itemDataManger;
    private readonly ICacheManager _cacheManager;
    private readonly IDatabase _database;

    public OpenGiftEvent(IItemDataManager itemDataManager, ICacheManager cacheManager , IDatabase database)
    {
        _itemDataManger = itemDataManager;
        _cacheManager = cacheManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var presentId = packet.PopInt();
        var present = room.GetRoomItemHandler().GetItem(presentId);
        if (present == null)
            return Task.CompletedTask;
        if (present.UserId != session.GetHabbo().Id)
            return Task.CompletedTask;
        DataRow data;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `base_id`,`extra_data` FROM `user_presents` WHERE `item_id` = @presentId LIMIT 1");
            dbClient.AddParameter("presentId", present.Id);
            data = dbClient.GetRow();
        }
        if (data == null)
        {
            session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
            room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
            }
            session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
            return Task.CompletedTask;
        }
        if (!int.TryParse(present.ExtraData.Split(Convert.ToChar(5))[2], out var purchaserId))
        {
            session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
            room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
            }
            session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
            return Task.CompletedTask;
        }
        var purchaser = _cacheManager.GenerateUser(purchaserId);
        if (purchaser == null)
        {
            session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
            room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
            }
            session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
            return Task.CompletedTask;
        }
        if (!_itemDataManger.GetItem(Convert.ToInt32(data["base_id"]), out var baseItem))
        {
            session.SendNotification("Oops, it appears that the item within the gift is no longer in the hotel!");
            room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + present.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + present.Id + "' LIMIT 1");
            }
            session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
            return Task.CompletedTask;
        }
        present.MagicRemove = true;
        room.SendPacket(new ObjectUpdateComposer(present, Convert.ToInt32(session.GetHabbo().Id)));
        var thread = new Thread(() => FinishOpenGift(session, baseItem, present, room, data));
        thread.Start();
        return Task.CompletedTask;
    }

    private void FinishOpenGift(GameClient session, ItemData baseItem, Item present, Room room, DataRow row)
    {
        try
        {
            if (baseItem == null || present == null || room == null || row == null)
            Thread.Sleep(1500);
            var itemIsInRoom = true;
            room.GetRoomItemHandler().RemoveFurniture(session, present.Id);
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `base_item` = @BaseItem, `extra_data` = @edata WHERE `id` = @itemId LIMIT 1");
                dbClient.AddParameter("itemId", present.Id);
                dbClient.AddParameter("BaseItem", row["base_id"]);
                dbClient.AddParameter("edata", row["extra_data"]);
                dbClient.RunQuery();
                dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = " + present.Id + " LIMIT 1");
            }
            present.BaseItem = Convert.ToInt32(row["base_id"]);
            present.ResetBaseItem();
            present.ExtraData = !string.IsNullOrEmpty(Convert.ToString(row["extra_data"])) ? Convert.ToString(row["extra_data"]) : "";
            if (present.Data.Type == 's')
            {
                if (!room.GetRoomItemHandler().SetFloorItem(session, present, present.GetX, present.GetY, present.Rotation, true, false, true))
                {
                    using (var dbClient = _database.GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @itemId LIMIT 1");
                        dbClient.AddParameter("itemId", present.Id);
                        dbClient.RunQuery();
                    }
                    itemIsInRoom = false;
                }
            }
            else
            {
                using (var dbClient = _database.GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = @itemId LIMIT 1");
                    dbClient.AddParameter("itemId", present.Id);
                    dbClient.RunQuery();
                }
                itemIsInRoom = false;
            }
            session.SendPacket(new OpenGiftComposer(present.Data, present.ExtraData, present, itemIsInRoom));
            session.GetHabbo().GetInventoryComponent().UpdateItems(true);
        }
        catch
        {
            //ignored
        }
    }
}