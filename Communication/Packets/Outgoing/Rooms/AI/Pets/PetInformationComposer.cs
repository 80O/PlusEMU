using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;

internal class PetInformationComposer : ServerPacket
{
    public PetInformationComposer(Pet pet)
        : base(ServerPacketHeader.PetInformationMessageComposer)
    {
        Room room;
        if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(pet.RoomId, out room))
            return;
        WriteInteger(pet.PetId);
        WriteString(pet.Name);
        WriteInteger(pet.Level);
        WriteInteger(Pet.MaxLevel);
        WriteInteger(pet.Experience);
        WriteInteger(pet.ExperienceGoal);
        WriteInteger(pet.Energy);
        WriteInteger(Pet.MaxEnergy);
        WriteInteger(pet.Nutrition);
        WriteInteger(Pet.MaxNutrition);
        WriteInteger(pet.Respect);
        WriteInteger(pet.OwnerId);
        WriteInteger(pet.Age);
        WriteString(pet.OwnerName);
        WriteInteger(1); //3 on hab
        WriteBoolean(pet.Saddle > 0);
        WriteBoolean(false);
        WriteInteger(0); //5 on hab
        WriteInteger(pet.AnyoneCanRide); // Anyone can ride horse
        WriteInteger(0);
        WriteInteger(0); //512 on hab
        WriteInteger(0); //1536
        WriteInteger(0); //2560
        WriteInteger(0); //3584
        WriteInteger(0);
        WriteString("");
        WriteBoolean(false);
        WriteInteger(-1); //255 on hab
        WriteInteger(-1);
        WriteInteger(-1);
        WriteBoolean(false);
    }

    public PetInformationComposer(Habbo habbo)
        : base(ServerPacketHeader.PetInformationMessageComposer)
    {
        WriteInteger(habbo.Id);
        WriteString(habbo.Username);
        WriteInteger(habbo.Rank);
        WriteInteger(10);
        WriteInteger(0);
        WriteInteger(0);
        WriteInteger(100);
        WriteInteger(100);
        WriteInteger(100);
        WriteInteger(100);
        WriteInteger(habbo.GetStats().Respect);
        WriteInteger(habbo.Id);
        WriteInteger(Convert.ToInt32(Math.Floor((UnixTimestamp.GetNow() - habbo.AccountCreated) / 86400))); //How?
        WriteString(habbo.Username);
        WriteInteger(1); //3 on hab
        WriteBoolean(false);
        WriteBoolean(false);
        WriteInteger(0); //5 on hab
        WriteInteger(0); // Anyone can ride horse
        WriteInteger(0);
        WriteInteger(0); //512 on hab
        WriteInteger(0); //1536
        WriteInteger(0); //2560
        WriteInteger(0); //3584
        WriteInteger(0);
        WriteString("");
        WriteBoolean(false);
        WriteInteger(-1); //255 on hab
        WriteInteger(-1);
        WriteInteger(-1);
        WriteBoolean(false);
    }
}