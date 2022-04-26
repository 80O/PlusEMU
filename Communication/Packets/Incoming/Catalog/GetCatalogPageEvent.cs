using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class GetCatalogPageEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;

    public GetCatalogPageEvent(ICatalogManager catalogManager)
    {
        _catalogManager = catalogManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var pageId = packet.PopInt();
        packet.PopInt();
        var cataMode = packet.PopString();
        if (!_catalogManager.TryGetPage(pageId, out var page))
            return Task.CompletedTask;
        if (!page.Enabled || !page.Visible || page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1)
            return Task.CompletedTask;
        session.SendPacket(new CatalogPageComposer(page, cataMode));
        return Task.CompletedTask;
    }
}