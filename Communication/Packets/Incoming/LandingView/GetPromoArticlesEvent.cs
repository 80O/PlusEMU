using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.LandingView;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.LandingView;

namespace Plus.Communication.Packets.Incoming.LandingView;

internal class GetPromoArticlesEvent : IPacketEvent
{
    private readonly ILandingViewManager _landingViewManager;

    public GetPromoArticlesEvent(ILandingViewManager landingViewManager)
    {
        _landingViewManager = landingViewManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new PromoArticlesComposer(_landingViewManager.GetPromotionItems()));
        return Task.CompletedTask;
    }
}