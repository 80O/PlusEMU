using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks;
using Plus.Database;
using Plus.HabboHotel.GameClients;
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

    public async Task ConfirmLockEvent(GameClient session, uint pId, bool isConfirmed)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return;

        var item = room.GetRoomItemHandler().GetItem(pId);
        if (item == null || item.Definition == null || item.Definition.InteractionType != InteractionType.Lovelock)
            return;

        var userOneId = item.InteractingUser;
        var userTwoId = item.InteractingUser2;
        var userOne = room.GetRoomUserManager().GetRoomUserByHabbo(userOneId);
        var userTwo = room.GetRoomUserManager().GetRoomUserByHabbo(userTwoId);

        if (userOne == null && userTwo == null && userOne.GetClient() == null && userTwo.GetClient() == null)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            return;
        }
        else if (userOne == null)
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userTwo.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        else if (userTwo == null)
        {
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        else if (item.ExtraData.Serialize().Contains(Convert.ToChar(5).ToString()))
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("It appears this love lock has already been locked.");
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("It appears this love lock has already been locked.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        else if (!isConfirmed)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            userOne.LlPartner = 0;
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userTwo.CanWalk = true;
            return;
        }
        if (userOneId == session.GetHabbo().Id)
        {
            session.Send(new LoveLockDialogueSetLockedComposer(pId));
            userOne.LlPartner = userTwoId;
        }
        else if (userTwoId == session.GetHabbo().Id)
        {
            session.Send(new LoveLockDialogueSetLockedComposer(pId));
            userTwo.LlPartner = userOneId;
        }

        if (userOne.LlPartner == 0 || userTwo.LlPartner == 0)
            return;

        item.ExtraData.Store($"1{(char)5}{userOne.GetUsername()}{(char)5}{userTwo.GetUsername()}{(char)5}{userOne.GetClient().GetHabbo().Look}{(char)5}{userTwo.GetClient().GetHabbo().Look}{(char)5}{DateTime.Now:dd/MM/yyyy}");
        item.InteractingUser = 0;
        item.InteractingUser2 = 0;
        userOne.LlPartner = 0;
        userTwo.LlPartner = 0;
        item.UpdateState(true, true);
        await UpdateItemExtradata(item);
        userOne.GetClient().Send(new LoveLockDialogueCloseComposer(pId));
        userTwo.GetClient().Send(new LoveLockDialogueCloseComposer(pId));
        userOne.CanWalk = true;
        userTwo.CanWalk = true;
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

    public ItemDefinition? GetItemByName(string name)
    {
        foreach (var item in Items.Values)
        {
            if (item.ItemName == name)
                return item;
        }

        return null;
    }
}