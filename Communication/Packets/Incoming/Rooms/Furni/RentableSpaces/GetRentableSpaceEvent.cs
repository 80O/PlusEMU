using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces;

internal class GetRentableSpaceEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        packet.PopInt(); //unknown
        session.SendPacket(new RentableSpaceComposer());
        return Task.CompletedTask;
    }
}