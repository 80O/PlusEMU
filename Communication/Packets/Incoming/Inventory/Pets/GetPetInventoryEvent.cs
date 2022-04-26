using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Pets;

internal class GetPetInventoryEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().GetInventoryComponent() == null)
            return Task.CompletedTask;
        var pets = session.GetHabbo().GetInventoryComponent().GetPets();
        session.SendPacket(new PetInventoryComposer(pets));
        return Task.CompletedTask;
    }
}