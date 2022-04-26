using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.GameCenter;

internal class InitializeGameCenterEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet) => Task.CompletedTask;
}