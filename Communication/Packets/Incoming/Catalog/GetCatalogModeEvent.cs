using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetCatalogModeEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        // string mode = packet.PopString();
        return Task.CompletedTask;
    }
}