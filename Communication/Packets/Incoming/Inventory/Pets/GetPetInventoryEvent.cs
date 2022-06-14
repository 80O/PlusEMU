using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Pets;

internal class GetPetInventoryEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().Inventory == null)
            return Task.CompletedTask;
        var pets = session.GetHabbo().Inventory.Pets.Pets.Values.ToList();
        session.SendPacket(new PetInventoryComposer(pets));
        return Task.CompletedTask;
    }
}