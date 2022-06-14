using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog;

internal class GetCatalogModeEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        string mode = packet.PopString();
        session.SendPacket(new CatalogIndexComposer(session, PlusEnvironment.GetGame().GetCatalog().GetPages()));
        return Task.CompletedTask;
    }
}