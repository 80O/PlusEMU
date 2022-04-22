﻿using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    class UseSellableClothingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            var itemId = packet.PopInt();

            var item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            if (item.Data == null)
                return;

            if (item.UserId != session.GetHabbo().Id)
                return;

            if (item.Data.InteractionType != InteractionType.PurchasableClothing)
            {
                session.SendNotification("Oops, this item isn't set as a sellable clothing item!");
                return;
            }

            if (item.Data.BehaviourData == 0)
            {
                session.SendNotification("Oops, this item doesn't have a linking clothing configuration, please report it!");
                return;
            }

            if (!PlusEnvironment.GetGame().GetCatalog().GetClothingManager().TryGetClothing(item.Data.BehaviourData, out var clothing))
            {
                session.SendNotification("Oops, we couldn't find this clothing part!");
                return;
            }

            //Quickly delete it from the database.
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
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
        }
    }
}
