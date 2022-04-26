using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class CanCreateRoomEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new CanCreateRoomComposer(false, 150));
        return Task.CompletedTask;
    }
}