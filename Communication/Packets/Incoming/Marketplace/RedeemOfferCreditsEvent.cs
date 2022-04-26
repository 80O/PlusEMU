using System;
using System.Data;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class RedeemOfferCreditsEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public RedeemOfferCreditsEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var creditsOwed = 0;
        DataTable table;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `asking_price` FROM `catalog_marketplace_offers` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `state` = '2'");
            table = dbClient.GetTable();
        }
        if (table != null)
        {
            foreach (DataRow row in table.Rows) creditsOwed += Convert.ToInt32(row["asking_price"]);
            if (creditsOwed >= 1)
            {
                session.GetHabbo().Credits += creditsOwed;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery("DELETE FROM `catalog_marketplace_offers` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `state` = '2'");
        }
        return Task.CompletedTask;
    }
}