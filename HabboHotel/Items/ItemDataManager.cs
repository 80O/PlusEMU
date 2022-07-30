using System.Data;
using Microsoft.Extensions.Logging;

namespace Plus.HabboHotel.Items;

public class ItemDataManager : IItemDataManager
{
    private readonly ILogger<ItemDataManager> _logger;
    public Dictionary<int, ItemDefinition> Gifts = new(0); //<SpriteId, Item>
    public Dictionary<int, ItemDefinition> Items = new(0);

    public ItemDataManager(ILogger<ItemDataManager> logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        if (Items.Count > 0)
            Items.Clear();
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `furniture`");
            var itemData = dbClient.GetTable();
            if (itemData != null)
            {
                foreach (DataRow row in itemData.Rows)
                {
                    try
                    {
                        var id = Convert.ToInt32(row["id"]);
                        var spriteId = Convert.ToInt32(row["sprite_id"]);
                        var itemName = Convert.ToString(row["item_name"]);
                        var publicName = Convert.ToString(row["public_name"]);
                        var type = row["type"].ToString();
                        var width = Convert.ToInt32(row["width"]);
                        var length = Convert.ToInt32(row["length"]);
                        var height = Convert.ToDouble(row["stack_height"]);
                        var allowStack = row["can_stack"].ToString() == "1";
                        var allowWalk = row["is_walkable"].ToString() == "1";
                        var allowSit =row["can_sit"].ToString() == "1";
                        var allowRecycle = row["allow_recycle"].ToString() == "1";
                        var allowTrade = row["allow_trade"].ToString() == "1";
                        var allowMarketplace = row["allow_marketplace_sell"].ToString() == "1";
                        var allowGift = row["allow_gift"].ToString()    == "1";
                        var allowInventoryStack = row["allow_inventory_stack"].ToString() == "1";
                        var interactionType = InteractionTypes.GetTypeFromString(row["interaction_type"].ToString());
                        var behaviourData = Convert.ToInt32(row["behaviour_data"]);
                        var cycleCount = Convert.ToInt32(row["interaction_modes_count"]);
                        var vendingIds = Convert.ToString(row["vending_ids"]);
                        var heightAdjustable = Convert.ToString(row["height_adjustable"]);
                        var effectId = Convert.ToInt32(row["effect_id"]);
                        var isRare = row["is_rare"].ToString() == "1";
                        var extraRot = row["extra_rot"].ToString() == "1";
                        if (!Gifts.ContainsKey(spriteId))
                        {
                            Gifts.Add(spriteId,
                                new ItemDefinition(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift,
                                    allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));
                        }
                        if (!Items.ContainsKey(id))
                        {
                            Items.Add(id,
                                new ItemDefinition(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift,
                                    allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.ReadKey();
                        //Logging.WriteLine("Could not load item #" + Convert.ToInt32(Row[0]) + ", please verify the data is okay.");
                    }
                }
            }
        }
        _logger.LogInformation("Item Manager -> LOADED");
    }

    [Obsolete("Cannot use this in an async context!")]
    public bool GetItem(int id, out ItemDefinition item)
    {
        if (Items.TryGetValue(id, out item))
            return true;
        return false;
    }

    public ItemDefinition? GetItemData(int id)
    {
        if (Items.TryGetValue(id, out var item))
            return item;
        return null;
    }

    public ItemDefinition GetItemByName(string name)
    {
        foreach (var entry in Items)
        {
            var item = entry.Value;
            if (item.ItemName == name)
                return item;
        }
        return null;
    }

    public bool GetGift(int spriteId, out ItemDefinition item)
    {
        if (Gifts.TryGetValue(spriteId, out item))
            return true;
        return false;
    }
}