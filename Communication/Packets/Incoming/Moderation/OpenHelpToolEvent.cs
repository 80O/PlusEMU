using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class OpenHelpToolEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new OpenHelpToolComposer());
        return Task.CompletedTask;
    }
}