using System.Threading.Tasks;
using Plus.Communication.Attributes;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
internal class PingEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.PingCount = 0;
        return Task.CompletedTask;
    }
}