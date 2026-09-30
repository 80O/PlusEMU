using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Inventory.Furniture;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Marketplace;

internal class MakeOfferEvent : IPacketEvent
{
    private readonly IMarketplaceManager _marketplaceManager;
    private readonly IDatabase _database;
    private readonly ISettingsManager _settingsManager;

    public MakeOfferEvent(IMarketplaceManager marketplaceManager, IDatabase database, ISettingsManager settingsManager)
    {
        _marketplaceManager = marketplaceManager;
        _database = database;
        _settingsManager = settingsManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var sellingPrice = packet.ReadInt();
        packet.ReadInt(); //comission
        var itemId = packet.ReadUInt();
        var item = session.GetHabbo().Inventory.Furniture.GetItem(itemId);
        if (item == null)
        {
            session.Send(new MarketplaceMakeOfferResultComposer(0));
            return Task.CompletedTask;
        }
        if (_settingsManager.TryGetValue("catalog.marketplace.rares_only") == "1" && !ItemUtility.IsRare(item))
        {
            session.SendNotification("Sorry, only Rares & LTDs can be auctioned in the Marketplace!");
            session.Send(new MarketplaceMakeOfferResultComposer(0));
            return Task.CompletedTask;
        }
        if (sellingPrice > 70000000 || sellingPrice == 0)
        {
            session.Send(new MarketplaceMakeOfferResultComposer(0));
            return Task.CompletedTask;
        }
        var comission = _marketplaceManager.CalculateComissionPrice(sellingPrice);
        var totalPrice = sellingPrice + comission;
        var itemType = 1;
        if (item.Definition.Type == ItemType.Wall)
            itemType++;
        using (var dbClient = _database.GetQueryReactor())
        {
            // TODO @80O: Do not delete items from the items table when putting on marketplace. Just reference the furniture instead.
            dbClient.SetQuery(
                $"INSERT INTO `catalog_marketplace_offers` (`furni_id`,`item_id`,`user_id`,`asking_price`,`total_price`,`public_name`,`sprite_id`,`item_type`,`timestamp`,`extra_data`,`limited_number`,`limited_stack`) VALUES ('{itemId}','{item.Definition.Id}','{session.GetHabbo().Id}','{sellingPrice}','{totalPrice}',@public_name,'{item.Definition.SpriteId}','{itemType}','{UnixTimestamp.GetNow()}',@extra_data, '{item.UniqueNumber}', '{item.UniqueSeries}')");
            dbClient.AddParameter("public_name", item.Definition.PublicName);
            dbClient.AddParameter("extra_data", item.ExtraData);
            dbClient.RunQuery();
            dbClient.RunQuery($"DELETE FROM `items` WHERE `id` = '{itemId}' AND `user_id` = '{session.GetHabbo().Id}' LIMIT 1");
        }
        session.GetHabbo().Inventory.Furniture.RemoveItem(itemId);
        session.Send(new FurniListRemoveComposer(itemId));
        session.Send(new MarketplaceMakeOfferResultComposer(1));
        return Task.CompletedTask;
    }
}