using System;
using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class GetMarketplaceItemStatsEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public GetMarketplaceItemStatsEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var itemId = packet.PopInt();
        var spriteId = packet.PopInt();
        DataRow row;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
            dbClient.AddParameter("SpriteId", spriteId);
            row = dbClient.GetRow();
        }
        session.SendPacket(new MarketplaceItemStatsComposer(itemId, spriteId, row != null ? Convert.ToInt32(row["avgprice"]) : 0));
        return Task.CompletedTask;
    }
}