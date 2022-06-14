using Dapper;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class PurchaseRoomPromotionEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;
    private readonly IDatabase _database;
    private readonly IBadgeManager _badgeManager;
    private readonly IMessengerDataLoader _messengerDataLoader;

    public PurchaseRoomPromotionEvent(IWordFilterManager wordFilterManager, IDatabase database, IBadgeManager badgeManager, IMessengerDataLoader messengerDataLoader)
    {
        _wordFilterManager = wordFilterManager;
        _database = database;
        _badgeManager = badgeManager;
        _messengerDataLoader = messengerDataLoader;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        packet.PopInt(); //pageId
        packet.PopInt(); //itemId
        var roomId = packet.PopInt();
        var name = _wordFilterManager.CheckMessage(packet.PopString());
        packet.PopBoolean(); //junk
        var desc = _wordFilterManager.CheckMessage(packet.PopString());
        var categoryId = packet.PopInt();
        if (!RoomFactory.TryGetData(roomId, out var data))
            return;
        if (data.OwnerId != session.GetHabbo().Id)
            return;
        if (data.Promotion == null)
            data.Promotion = new RoomPromotion(name, desc, categoryId);
        else
        {
            data.Promotion.Name = name;
            data.Promotion.Description = desc;
            data.Promotion.TimestampExpires += 7200;
        }
        using (var connection = _database.Connection())
        {
            connection.Execute(
                "REPLACE INTO `room_promotions` (`room_id`,`title`,`description`,`timestamp_start`,`timestamp_expire`,`category_id`) VALUES (@roomId, @title, @description, @start, @expires, @categoryId)",
                new { roomId = roomId, title = name, description = desc, start = data.Promotion.TimestampStarted, expires = data.Promotion.TimestampExpires, categoryId = categoryId });
        }
        if (!session.GetHabbo().Inventory.Badges.HasBadge("RADZZ"))
            await _badgeManager.GiveBadge(session.GetHabbo(), "RADZZ");
        session.SendPacket(new PurchaseOkComposer());
        if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoomId == roomId)
            session.GetHabbo().CurrentRoom?.SendPacket(new RoomEventComposer(data, data.Promotion));
        _messengerDataLoader.BroadcastStatusUpdate(session.GetHabbo(), MessengerEventTypes.EventStarted, name);
    }
}