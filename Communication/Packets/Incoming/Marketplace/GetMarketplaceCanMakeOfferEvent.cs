using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class GetMarketplaceCanMakeOfferEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var errorCode = session.GetHabbo().TradingLockExpiry > 0 ? 6 : 1;
        session.SendPacket(new MarketplaceCanMakeOfferResultComposer(errorCode));
        return Task.CompletedTask;
    }
}