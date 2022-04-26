using System;
using System.Drawing;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Pets.Locale;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;

internal class RideHorseEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IPetLocale _petLocale;

    public RideHorseEvent(IRoomManager roomManager, IPetLocale petLocale)
    {
        _roomManager = roomManager;
        _petLocale = petLocale;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        var petId = packet.PopInt();
        var type = packet.PopBoolean();
        if (!room.GetRoomUserManager().TryGetPet(petId, out var pet))
            return Task.CompletedTask;
        if (pet.PetData == null)
            return Task.CompletedTask;
        if (pet.PetData.AnyoneCanRide == 0 && pet.PetData.OwnerId != user.UserId)
        {
            session.SendNotification(
                "You are unable to ride this horse.\nThe owner of the pet has not selected for anyone to ride it.");
            return Task.CompletedTask;
        }
        if (type)
        {
            if (pet.RidingHorse)
            {
                var speech2 = _petLocale.GetValue("pet.alreadymounted");
                pet.Chat(speech2[Random.Shared.Next(0, speech2.Length)]);
            }
            else if (user.RidingHorse)
                session.SendNotification("You are already riding a horse!");
            else
            {
                if (pet.Statusses.Count > 0)
                    pet.Statusses.Clear();
                var newX2 = user.X;
                var newY2 = user.Y;
                room.SendPacket(room.GetRoomItemHandler().UpdateUserOnRoller(pet, new Point(newX2, newY2), 0, room.GetGameMap().SqAbsoluteHeight(newX2, newY2)));
                room.SendPacket(room.GetRoomItemHandler().UpdateUserOnRoller(user, new Point(newX2, newY2), 0, room.GetGameMap().SqAbsoluteHeight(newX2, newY2) + 1));
                user.MoveTo(newX2, newY2);
                pet.ClearMovement(true);
                user.RidingHorse = true;
                pet.RidingHorse = true;
                pet.HorseId = user.VirtualId;
                user.HorseId = pet.VirtualId;
                user.ApplyEffect(77);
                user.RotBody = pet.RotBody;
                user.RotHead = pet.RotHead;
                user.UpdateNeeded = true;
                pet.UpdateNeeded = true;
            }
        }
        else
        {
            if (user.VirtualId == pet.HorseId)
            {
                pet.Statusses.Remove("sit");
                pet.Statusses.Remove("lay");
                pet.Statusses.Remove("snf");
                pet.Statusses.Remove("eat");
                pet.Statusses.Remove("ded");
                pet.Statusses.Remove("jmp");
                user.RidingHorse = false;
                user.HorseId = 0;
                pet.RidingHorse = false;
                pet.HorseId = 0;
                user.MoveTo(new Point(user.X + 2, user.Y + 2));
                user.ApplyEffect(-1);
                user.UpdateNeeded = true;
                pet.UpdateNeeded = true;
            }
            else
                session.SendNotification("Could not dismount this horse - You are not riding it!");
        }
        room.SendPacket(new PetHorseFigureInformationComposer(pet));
        return Task.CompletedTask;
    }
}