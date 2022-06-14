using Plus.HabboHotel.Rooms.AI;

namespace Plus.Communication.Packets.Outgoing.Inventory.Pets;

internal class PetInventoryComposer : ServerPacket
{
    public PetInventoryComposer(ICollection<Pet> pets)
        : base(ServerPacketHeader.PetInventoryMessageComposer)
    {
        WriteInteger(1);
        WriteInteger(1);
        WriteInteger(pets.Count);
        foreach (var pet in pets.ToList())
        {
            WriteInteger(pet.PetId);
            WriteString(pet.Name);
            WriteInteger(pet.Type);
            WriteInteger(int.Parse(pet.Race));
            WriteString(pet.Color);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}