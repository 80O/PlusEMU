using System.Threading.Tasks;
using Plus.Communication.Attributes;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class GetClientVersionEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var build = packet.PopString();
        if (PlusEnvironment.SwfRevision != build)
            PlusEnvironment.SwfRevision = build;
        return Task.CompletedTask;
    }
}