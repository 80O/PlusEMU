using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Bots;

internal class GetBotInventoryEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().GetInventoryComponent() == null)
            return Task.CompletedTask;
        var bots = session.GetHabbo().GetInventoryComponent().GetBots();
        session.SendPacket(new BotInventoryComposer(bots));
        return Task.CompletedTask;
    }
}