using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Misc;

internal class LatencyTestEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        //Session.SendMessage(new LatencyTestComposer(Packet.PopInt()));
        return Task.CompletedTask;
    }
}