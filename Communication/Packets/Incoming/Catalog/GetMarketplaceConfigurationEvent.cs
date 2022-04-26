using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class GetMarketplaceConfigurationEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new MarketplaceConfigurationComposer());
        return Task.CompletedTask;
    }
}