using Plus.HabboHotel.Catalog.Pets;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public class SellablePetBreedsComposer : ServerPacket
{
    public SellablePetBreedsComposer(string petType, int petId, ICollection<PetRace> races)
        : base(ServerPacketHeader.SellablePetBreedsMessageComposer)
    {
        WriteString(petType);
        WriteInteger(races.Count);
        foreach (var race in races.ToList())
        {
            WriteInteger(petId);
            WriteInteger(race.PrimaryColour);
            WriteInteger(race.SecondaryColour);
            WriteBoolean(race.HasPrimaryColour);
            WriteBoolean(race.HasSecondaryColour);
        }
    }
}