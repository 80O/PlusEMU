using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Polls;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Polls;

internal class PollStartEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new PollContentsComposer());
        return Task.CompletedTask;
    }
}