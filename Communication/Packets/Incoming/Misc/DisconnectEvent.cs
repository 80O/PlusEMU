using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Misc;

internal class DisconnectEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.Disconnect();
        return Task.CompletedTask;
    }
}