using System.Threading.Tasks;
using Plus.Communication.Attributes;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class SsoTicketEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var sso = packet.PopString();
        if (string.IsNullOrEmpty(sso)) //|| sso.Length < 15)
            return Task.CompletedTask;
        
        session.TryAuthenticate(sso);
        return Task.CompletedTask;
    }
}