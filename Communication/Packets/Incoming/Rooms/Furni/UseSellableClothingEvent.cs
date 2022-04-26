using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Database;
using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class UseSellableClothingEvent : IPacketEvent
{
    private readonly IClothingManager _clothingManager;
    private readonly IDatabase _database;

    public UseSellableClothingEvent(IClothingManager clothingManager, IDatabase database)
    {
        _clothingManager = clothingManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        if (item.Data == null)
            return Task.CompletedTask;
        if (item.UserId != session.GetHabbo().Id)
            return Task.CompletedTask;
        if (item.Data.InteractionType != InteractionType.PurchasableClothing)
        {
            session.SendNotification("Oops, this item isn't set as a sellable clothing item!");
            return Task.CompletedTask;
        }
        if (item.Data.BehaviourData == 0)
        {
            session.SendNotification("Oops, this item doesn't have a linking clothing configuration, please report it!");
            return Task.CompletedTask;
        }
        if (!_clothingManager.TryGetClothing(item.Data.BehaviourData, out var clothing))
        {
            session.SendNotification("Oops, we couldn't find this clothing part!");
            return Task.CompletedTask;
        }

        //Quickly delete it from the database.
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1");
            dbClient.AddParameter("ItemId", item.Id);
            dbClient.RunQuery();
        }

        //Remove the item.
        room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
        session.GetHabbo().GetClothing().AddClothing(clothing.ClothingName, clothing.PartIds);
        session.SendPacket(new FigureSetIdsComposer(session.GetHabbo().GetClothing().GetClothingParts));
        session.SendPacket(new RoomNotificationComposer("figureset.redeemed.success"));
        session.SendWhisper("If for some reason cannot see your new clothing, reload the hotel!");
        return Task.CompletedTask;
    }
}