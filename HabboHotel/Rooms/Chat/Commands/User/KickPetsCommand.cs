using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class KickPetsCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    private readonly IDatabase _database;
    public string Key => "kickpets";
    public string PermissionRequired => "command_kickpets";

    public string Parameters => "";

    public string Description => "Kick all of the pets from the room.";

    public KickPetsCommand(IGameClientManager gameClientManager, IDatabase database)
    {
        _gameClientManager = gameClientManager;
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (!room.CheckRights(session, true))
        {
            session.SendWhisper("Oops, only the room owner can run this command!");
            return;
        }
        if (room.GetRoomUserManager().GetPets().Count == 0) session.SendWhisper("Oops, there isn't any pets in here!?");
        foreach (var bot in room.GetRoomUserManager().GetUserList().ToList())
        {
            if (bot == null)
                continue;
            if (bot.RidingHorse)
            {
                var rider = room.GetRoomUserManager().GetRoomUserByVirtualId(bot.HorseId);
                if (rider != null)
                {
                    rider.RidingHorse = false;
                    rider.ApplyEffect(-1);
                    rider.MoveTo(new Point(rider.X + 1, rider.Y + 1));
                }
                else
                    bot.RidingHorse = false;
            }
            var pet = bot.PetData;
            if (pet != null) return;
            pet.RoomId = 0;
            pet.PlacedInRoom = false;
            room.GetRoomUserManager().RemoveBot(bot.VirtualId, false);
            if (pet.OwnerId != session.GetHabbo().Id)
            {
                var targetClient = _gameClientManager.GetClientByUserId(pet.OwnerId);
                if (targetClient != null)
                    if (targetClient.GetHabbo().Inventory.Pets.AddPet(pet))
                        targetClient.SendPacket(new PetInventoryComposer(targetClient.GetHabbo().Inventory.Pets.Pets.Values.ToList()));
            }
            if (session.GetHabbo().Inventory.Pets.AddPet(pet)) session.SendPacket(new PetInventoryComposer(session.GetHabbo().Inventory.Pets.Pets.Values.ToList()));
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery("UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" + pet.PetId + "' LIMIT 1");
            dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + pet.Experience + "', `energy` = '" + pet.Energy + "', `nutrition` = '" + pet.Nutrition + "', `respect` = '" + pet.Respect +
                              "' WHERE `id` = '" + pet.PetId + "' LIMIT 1");
        }
        session.SendWhisper("All pets have been kicked from the room.");
    }
}