using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class RemoveSaddleFromHorseEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IItemDataManager _itemDataManager;
    private readonly IDatabase _database;

    public RemoveSaddleFromHorseEvent(IRoomManager roomManager, IItemDataManager itemDataManager ,IDatabase database)
    {
        _roomManager = roomManager;
        _itemDataManager = itemDataManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.GetRoomUserManager().TryGetPet(packet.PopInt(), out var petUser))
            return Task.CompletedTask;
        if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
            return Task.CompletedTask;

        //Fetch the furniture Id for the pets current saddle.
        var saddleId = ItemUtility.GetSaddleId(petUser.PetData.Saddle);

        //Remove the saddle from the pet.
        petUser.PetData.Saddle = 0;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '0' WHERE `id` = '" + petUser.PetData.PetId + "' LIMIT 1");
        }

        //Give the saddle back to the user.
        if (!_itemDataManager.GetItem(saddleId, out var itemData))
            return Task.CompletedTask;
        var item = ItemFactory.CreateSingleItemNullable(itemData, session.GetHabbo(), "", "");
        if (item != null)
        {
            session.GetHabbo().GetInventoryComponent().TryAddItem(item);
            session.SendPacket(new FurniListNotificationComposer(item.Id, 1));
            session.SendPacket(new PurchaseOkComposer());
            session.SendPacket(new FurniListAddComposer(item));
            session.SendPacket(new FurniListUpdateComposer());
        }

        //Update the Pet and the Pet figure information.
        room.SendPacket(new UsersComposer(petUser));
        room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        return Task.CompletedTask;
    }
}