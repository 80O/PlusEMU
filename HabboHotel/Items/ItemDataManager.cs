using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Database;
using Plus.HabboHotel.Users.Inventory.Furniture;

namespace Plus.HabboHotel.Items;

public class ItemDataManager : IItemDataManager
{
    private readonly ILogger<ItemDataManager> _logger;
    private readonly IDatabase _database;
    public Dictionary<int, uint> Gifts { get; } = new(0); //<SpriteId, Item>
    public Dictionary<uint, ItemDefinition> Items { get; } = new(0);

    public ItemDataManager(ILogger<ItemDataManager> logger, IDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    public async Task UpdateItemExtradata(Item item)
    {
        using var connection = _database.Connection();
        await connection.ExecuteAsync("UPDATE `items` SET `extra_data` = @extraData WHERE `id` = @ID LIMIT 1", new
        {
            extraData = item.ExtraData,
            ID = item.Id
        });
    }

    public void Init()
    {
        if (Items.Count > 0)
            Items.Clear();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `furniture`");
            var itemData = dbClient.GetTable();
            if (itemData != null)
            {
                foreach (DataRow row in itemData.Rows)
                {
                    try
                    {
                        var definition = new ItemDefinition
                        {
                            Id = Convert.ToUInt32(row["id"]),
                            SpriteId = Convert.ToInt32(row["sprite_id"]),
                            ItemName = Convert.ToString(row["item_name"]),
                            PublicName = Convert.ToString(row["public_name"]),
                            Type = string.Equals(row["type"].ToString(), "s", StringComparison.OrdinalIgnoreCase) ? ItemType.Floor : ItemType.Wall,
                            Width = Convert.ToInt32(row["width"]),
                            Length = Convert.ToInt32(row["length"]),
                            Height = Convert.ToDouble(row["stack_height"]),
                            Stackable = row["can_stack"].ToString() == "1",
                            Walkable = row["is_walkable"].ToString() == "1",
                            IsSeat = row["can_sit"].ToString() == "1",
                            AllowEcotronRecycle = row["allow_recycle"].ToString() == "1",
                            AllowTrade = row["allow_trade"].ToString() == "1",
                            AllowMarketplaceSell = row["allow_marketplace_sell"].ToString() == "1",
                            AllowGift = row["allow_gift"].ToString() == "1",
                            AllowInventoryStack = row["allow_inventory_stack"].ToString() == "1",
                            InteractionType = InteractionTypes.GetTypeFromString(row["interaction_type"].ToString()),
                            BehaviourData = Convert.ToInt32(row["behaviour_data"]),
                            Modes = Convert.ToInt32(row["interaction_modes_count"]),
                            VendingIds = (!string.IsNullOrEmpty(Convert.ToString(row["vending_ids"])) && Convert.ToString(row["vending_ids"]) != "0")
                                ? Convert.ToString(row["vending_ids"]).Split(",").Select(int.Parse).ToList()
                                : new(0),
                            AdjustableHeights = (!string.IsNullOrEmpty(Convert.ToString(row["height_adjustable"])) && Convert.ToString(row["height_adjustable"]) != "0")
                                ? Convert.ToString(row["height_adjustable"]).Split(",").Select(double.Parse).ToList()
                                : new(0),
                            EffectId = Convert.ToInt32(row["effect_id"]),
                            IsRare = row["is_rare"].ToString() == "1",
                            ExtraRot = row["extra_rot"].ToString() == "1",
                        };

                        Gifts.TryAdd(definition.SpriteId, definition.Id);
                        Items.Add(definition.Id, definition);
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
}