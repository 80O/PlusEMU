using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class CheckGnomeNameEvent : IPacketEvent
{
    private readonly IDatabase _database;
    private readonly IItemDataManager _itemDataManager;

    public CheckGnomeNameEvent(IDatabase database, IItemDataManager itemDataManager)
    {
        _database = database;
        _itemDataManager = itemDataManager;
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
        if (item == null || item.Data == null || item.UserId != session.GetHabbo().Id || item.Data.InteractionType != InteractionType.GnomeBox)
            return Task.CompletedTask;
        var petName = packet.PopString();
        if (string.IsNullOrEmpty(petName))
        {
            session.SendPacket(new CheckGnomeNameComposer(petName, 1));
            return Task.CompletedTask;
        }
        if (!PetUtility.CheckPetName(petName))
        {
            session.SendPacket(new CheckGnomeNameComposer(petName, 1));
            return Task.CompletedTask;
        }
        var x = item.GetX;
        var y = item.GetY;

        //Quickly delete it from the database.
        using (var connection = _database.Connection())
        {
            connection.Execute("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1", new { ItemId = item.Id});
        }

        //Remove the item.
        room.GetRoomItemHandler().RemoveFurniture(session, item.Id);

        //Apparently we need this for success.
        session.SendPacket(new CheckGnomeNameComposer(petName, 0));

        //Create the pet here.
        var pet = PetUtility.CreatePet(session.GetHabbo().Id, petName, 26, "30", "ffffff");
        if (pet == null)
        {
            session.SendNotification("Oops, an error occoured. Please report this!");
            return Task.CompletedTask;
        }
        var rndSpeechList = new List<RandomSpeech>();
        pet.RoomId = session.GetHabbo().CurrentRoomId;
        pet.GnomeClothing = RandomClothing();

        //Update the pets gnome clothing.
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `bots_petdata` SET `gnome_clothing` = @GnomeClothing WHERE `id` = @PetId LIMIT 1", new { GnomeClothing = pet.GnomeClothing, PetId = pet.PetId });
        }

        //Make a RoomUser of the pet.
        room.GetRoomUserManager()
            .DeployBot(new RoomBot(pet.PetId, pet.RoomId, "pet", "freeroam", pet.Name, "", pet.Look, x, y, 0, 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0), pet);

        //Give the food.
        if (_itemDataManager.GetItem(320, out var petFood))
        {
            var food = ItemFactory.CreateSingleItemNullable(petFood, session.GetHabbo(), "", "");
            if (food != null)
            {
                session.GetHabbo().GetInventoryComponent().TryAddItem(food);
                session.SendPacket(new FurniListNotificationComposer(food.Id, 1));
            }
        }
        return Task.CompletedTask;
    }

    private static string RandomClothing()
    {
        var randomNumber = Random.Shared.Next(1, 7);
        switch (randomNumber)
        {
            default:
                return "5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
            case 2:
                return "5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
            case 3:
                return "5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
            case 4:
                return "5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
            case 5:
                return "5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24";
            case 6:
                return "5 4 402 5 3 302 21 0 -1 7 1 101 12 2 201 17";
        }
    }
}