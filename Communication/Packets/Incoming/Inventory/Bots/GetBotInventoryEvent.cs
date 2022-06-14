using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Bots;

internal class GetBotInventoryEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().Inventory == null)
            return Task.CompletedTask;
        var bots = session.GetHabbo().Inventory.Bots.Bots.Values.ToList();
        session.SendPacket(new BotInventoryComposer(bots));
        return Task.CompletedTask;
    }
}