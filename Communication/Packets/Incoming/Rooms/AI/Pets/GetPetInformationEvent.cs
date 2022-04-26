using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets;

internal class GetPetInformationEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var petId = packet.PopInt();
        if (!session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(petId, out var pet))
        {
            //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
            var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
            if (user == null)
                return Task.CompletedTask;

            //Check some values first, please!
            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return Task.CompletedTask;

            //And boom! Let us send the information composer 8-).
            session.SendPacket(new PetInformationComposer(user.GetClient().GetHabbo()));
            return Task.CompletedTask;
        }

        //Continue as a regular pet..
        if (pet.RoomId != session.GetHabbo().CurrentRoomId || pet.PetData == null)
            return Task.CompletedTask;
        session.SendPacket(new PetInformationComposer(pet.PetData));
        return Task.CompletedTask;
    }
}