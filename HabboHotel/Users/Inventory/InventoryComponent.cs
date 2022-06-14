using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users.Inventory.Badges;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Users.Inventory.Furniture;
using Plus.HabboHotel.Users.Inventory.Pets;

namespace Plus.HabboHotel.Users.Inventory;

public class InventoryComponent
{
    public BadgesInventoryComponent Badges { get; set; }
    public FurnitureInventoryComponent Furniture { get; set; }
    public PetsInventoryComponent Pets { get; set; }
    public BotInventoryComponent Bots { get; set; }

    [Obsolete("Should be removed when the mess below is refactored")]
    public int UserId { get; set; }

    [Obsolete("Should be removed when the mess below is refactored")]
    public GameClient Client { get; set; }

    [Obsolete]
    public Item AddNewItem(int id, int baseItem, string extraData, int group, bool toInsert, bool fromRoom, int limitedNumber, int limitedStack)
    {
        /// WTF IS THIS SHIT
        if (toInsert)
        {
            if (fromRoom)
            {
                using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                dbClient.RunQuery("UPDATE `items` SET `room_id` = '0', `user_id` = '" + UserId + "' WHERE `id` = '" + id + "' LIMIT 1");
            }
            else
            {
                using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                if (id > 0)
                {
                    dbClient.RunQuery("INSERT INTO `items` (`id`,`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" + id + "', '" + baseItem + "', '" + UserId + "', '" +
                                      limitedNumber + "', '" + limitedStack + "')");
                }
                else
                {
                    dbClient.SetQuery("INSERT INTO `items` (`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" + baseItem + "', '" + UserId + "', '" + limitedNumber + "', '" +
                                      limitedStack + "')");
                    id = Convert.ToInt32(dbClient.InsertQuery());
                }
                SendNewItems(Convert.ToInt32(id));
                if (group > 0)
                    dbClient.RunQuery("INSERT INTO `items_groups` VALUES (" + id + ", " + group + ")");
                if (!string.IsNullOrEmpty(extraData))
                {
                    dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extradata WHERE `id` = '" + id + "' LIMIT 1");
                    dbClient.AddParameter("extradata", extraData);
                    dbClient.RunQuery();
                }
            }
        }
        var itemToAdd = new Item(id, 0, baseItem, extraData, 0, 0, 0, 0, UserId, group, limitedNumber, limitedStack, string.Empty);
        Furniture.RemoveItem(id);
        Furniture.AddItem(itemToAdd);
        return itemToAdd;
    }

    public void SendNewItems(int id)
    {
        Client.SendPacket(new FurniListNotificationComposer(id, 1));
    }
}