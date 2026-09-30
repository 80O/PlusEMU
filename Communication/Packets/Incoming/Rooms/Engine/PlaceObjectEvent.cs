using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Core.Settings;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class PlaceObjectEvent : RoomPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IAchievementManager _achievementManager;

    public PlaceObjectEvent(IRoomManager roomManager, ISettingsManager settingsManager, IAchievementManager achievementManager)
    {
        _roomManager = roomManager;
        _settingsManager = settingsManager;
        _achievementManager = achievementManager;
    }

    /// TODO @80O: Unfuck this mess
    public override Task Parse(Room room, GameClient session, IIncomingPacket packet)
    {
        lock (session.GetHabbo().Inventory.Furniture.SyncRoot)
        {
            var rawData = packet.ReadString();
            var data = rawData.Split(' ');
            if (!uint.TryParse(data[0], out var itemId))
                return Task.CompletedTask;
            var hasRights = room.CheckRights(session, false, true);
            if (!hasRights)
            {
                session.Send(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_not_owner}"));
                return Task.CompletedTask;
            }
            if (room.GetRoomItemHandler().GetWallAndFloor.Count() > Convert.ToInt32(_settingsManager.TryGetValue("room.item.placement_limit")))
            {
                session.SendNotification($"You cannot have more than {Convert.ToInt32(_settingsManager.TryGetValue("room.item.placement_limit"))} items in a room!");
                return Task.CompletedTask;
            }
            var inventoryItem = session.GetHabbo().Inventory.Furniture.GetItem(itemId);
            if (inventoryItem == null)
                return Task.CompletedTask;
            var item = inventoryItem.ToRoomObject();
            if (item == null)
                return Task.CompletedTask;

            if (item.Definition.InteractionType == InteractionType.Exchange && room.OwnerId != session.GetHabbo().Id && !session.GetHabbo().Permissions.HasRight("room_item_place_exchange_anywhere"))
            {
                session.SendNotification("You cannot place exchange items in other people's rooms!");
                return Task.CompletedTask;
            }

            //TODO: Make neat.
            switch (item.Definition.InteractionType)
            {
                case InteractionType.Moodlight:
                {
                    var moodData = room.MoodlightData;
                    if (moodData != null && room.GetRoomItemHandler().GetItem(moodData.ItemId) != null)
                    {
                        session.SendNotification("You can only have one background moodlight per room!");
                        return Task.CompletedTask;
                    }
                    break;
                }
                case InteractionType.Toner:
                {
                    var tonerData = room.TonerData;
                    if (tonerData != null && room.GetRoomItemHandler().GetItem(tonerData.ItemId) != null)
                    {
                        session.SendNotification("You can only have one background toner per room!");
                        return Task.CompletedTask;
                    }
                    break;
                }
                case InteractionType.Hopper:
                {
                    if (room.GetRoomItemHandler().HopperCount > 0)
                    {
                        session.SendNotification("You can only have one hopper per room!");
                        return Task.CompletedTask;
                    }
                    break;
                }
                case InteractionType.Tent:
                case InteractionType.TentSmall:
                {
                    room.AddTent(item.Id);
                    break;
                }
            }
            if (!item.IsWallItem)
            {
                if (data.Length < 4)
                    return Task.CompletedTask;
                if (!int.TryParse(data[1], out var x)) return Task.CompletedTask;
                if (!int.TryParse(data[2], out var y)) return Task.CompletedTask;
                if (!int.TryParse(data[3], out var rotation)) return Task.CompletedTask;
                if (room.GetRoomItemHandler().SetFloorItem(session, item, x, y, rotation, true, false, true))
                {
                    session.GetHabbo().Inventory.Furniture.RemoveItem(itemId);
                    session.Send(new FurniListRemoveComposer(itemId));
                    if (session.GetHabbo().Id == room.OwnerId)
                        _achievementManager.ProgressAchievement(session, "ACH_RoomDecoFurniCount", 1);
                    if (item.IsWired)
                    {
                        try
                        {
                            room.GetWired().LoadWiredBox(item);
                        }
                        catch
                        {
                            Console.WriteLine(item.Definition.InteractionType);
                        }
                    }
                }
                else
                    session.Send(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
            }
            else if (item.IsWallItem)
            {
                var correctedData = new string[data.Length - 1];
                for (var i = 1; i < data.Length; i++) correctedData[i - 1] = data[i];
                var wallPos = room.GetRoomItemHandler().WallPositionCheck(string.Join(" ", correctedData));
                if (wallPos != null)
                {
                    item.WallCoordinates = wallPos;
                    try
                    {
                        if (room.GetRoomItemHandler().SetWallItem(session, item))
                        {
                            session.GetHabbo().Inventory.Furniture.RemoveItem(itemId);
                            session.Send(new FurniListRemoveComposer(itemId));
                            if (session.GetHabbo().Id == room.OwnerId)
                                _achievementManager.ProgressAchievement(session, "ACH_RoomDecoFurniCount", 1);
                        }
                    }
                    catch
                    {
                        session.Send(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
                    }
                }
                else
                    session.Send(new RoomNotificationComposer("furni_placement_error", "message", "${room.error.cant_set_item}"));
            }
            return Task.CompletedTask;
        }
    }
}
