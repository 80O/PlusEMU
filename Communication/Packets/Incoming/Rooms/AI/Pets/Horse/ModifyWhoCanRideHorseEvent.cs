using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class ModifyWhoCanRideHorseEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public ModifyWhoCanRideHorseEvent(IRoomManager roomManager, IDatabase database)
    {
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var petId = packet.PopInt();
        if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            return Task.CompletedTask;
        pet.PetData.AnyoneCanRide = pet.PetData.AnyoneCanRide == 1 ? 0 : 1;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `bots_petdata` SET `anyone_ride` = '" + pet.PetData.AnyoneCanRide + "' WHERE `id` = '" + petId + "' LIMIT 1");
        }
        room.SendPacket(new PetInformationComposer(pet.PetData));
        return Task.CompletedTask;
    }
}