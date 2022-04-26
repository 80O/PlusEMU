using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class GetCatalogIndexEvent : IPacketEvent
{
    private readonly ICatalogManager _catalogManager;

    public GetCatalogIndexEvent(ICatalogManager catalogManager)
    {
        _catalogManager = catalogManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new CatalogIndexComposer(session, _catalogManager.GetPages()));
        session.SendPacket(new CatalogItemDiscountComposer());
        session.SendPacket(new BcBorrowedItemsComposer());
        return Task.CompletedTask;
    }
}