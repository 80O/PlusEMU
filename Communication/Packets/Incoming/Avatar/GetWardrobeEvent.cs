using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Avatar;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Avatar;

internal class GetWardrobeEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new WardrobeComposer(session.GetHabbo().Id));
        return Task.CompletedTask;
    }
}