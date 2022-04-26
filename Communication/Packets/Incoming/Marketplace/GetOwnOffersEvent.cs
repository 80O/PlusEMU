using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class GetOwnOffersEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new MarketPlaceOwnOffersComposer(session.GetHabbo().Id));
        return Task.CompletedTask;
    }
}