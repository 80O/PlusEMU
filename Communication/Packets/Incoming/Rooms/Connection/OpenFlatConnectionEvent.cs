using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Connection;

public class OpenFlatConnectionEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var roomId = packet.PopInt();
        var password = packet.PopString();
        session.GetHabbo().PrepareRoom(roomId, password);
        return Task.CompletedTask;
    }
}