using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Misc;

internal class MemoryPerformanceEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet) => Task.CompletedTask;
}