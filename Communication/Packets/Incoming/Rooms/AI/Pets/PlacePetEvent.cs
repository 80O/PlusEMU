using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Core.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets;

internal class PlacePetEvent : IPacketEvent
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.Packets.Incoming.Rooms.AI.Pets.PlacePetEvent");
    private readonly IRoomManager _roomManager;
    private readonly ISettingsManager _settingsManager;

    public PlacePetEvent(IRoomManager roomManager, ISettingsManager settingsManager)
    {
        _roomManager = roomManager;
        _settingsManager = settingsManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (room.AllowPets == 0 && !room.CheckRights(session, true) || !room.CheckRights(session, true))
        {
            session.SendPacket(new RoomErrorNotifComposer(1));
            return Task.CompletedTask;
        }
        if (room.GetRoomUserManager().PetCount > Convert.ToInt32(_settingsManager.TryGetValue("room.pets.placement_limit")))
        {
            session.SendPacket(new RoomErrorNotifComposer(2)); //5 = I have too many.
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetInventoryComponent().TryGetPet(packet.PopInt(), out var pet))
            return Task.CompletedTask;
        if (pet == null)
            return Task.CompletedTask;
        if (pet.PlacedInRoom)
        {
            session.SendNotification("This pet is already in the room?");
            return Task.CompletedTask;
        }
        var x = packet.PopInt();
        var y = packet.PopInt();
        if (!room.GetGameMap().CanWalk(x, y, false))
        {
            session.SendPacket(new RoomErrorNotifComposer(4));
            return Task.CompletedTask;
        }
        if (room.GetRoomUserManager().TryGetPet(pet.PetId, out var oldPet)) room.GetRoomUserManager().RemoveBot(oldPet.VirtualId, false);
        pet.X = x;
        pet.Y = y;
        pet.PlacedInRoom = true;
        pet.RoomId = room.RoomId;
        var rndSpeechList = new List<RandomSpeech>();
        var roomBot = new RoomBot(pet.PetId, pet.RoomId, "pet", "freeroam", pet.Name, "", pet.Look, x, y, 0, 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0);
        room.GetRoomUserManager().DeployBot(roomBot, pet);
        pet.DbState = PetDatabaseUpdateState.NeedsUpdate;
        room.GetRoomUserManager().UpdatePets();
        if (!session.GetHabbo().GetInventoryComponent().TryRemovePet(pet.PetId, out var toRemove))
        {
            Log.Error("Error whilst removing pet: " + toRemove.PetId);
            return Task.CompletedTask;
        }
        session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
        return Task.CompletedTask;
    }
}