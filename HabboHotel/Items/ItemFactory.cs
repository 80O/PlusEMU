using Plus.Database;
using Plus.HabboHotel.Items.DataFormat;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Inventory.Furniture;

namespace Plus.HabboHotel.Items;

public class ItemFactory : IItemFactory
{
    private readonly IDatabase _database;
    public static ItemFactory Instance { get; set; }

    public ItemFactory(IDatabase database)
    {
        _database = database;
    }

    public Item CreateSingleItemNullable(ItemDefinition definition, Habbo habbo, string extraData, string displayFlags, int groupId = 0, uint limitedNumber = 0, uint limitedStack = 0)
    {
        if (definition == null) throw new InvalidOperationException("Data cannot be null.");
        var item = new Item()
        {
            OwnerId = (uint)habbo.Id,
            Definition = definition,
            ExtraData = CreateExtraData(definition, extraData),
            UniqueNumber = limitedNumber,
            UniqueSeries = limitedStack,
            GroupId = groupId
        };
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery(
            "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
        dbClient.AddParameter("did", definition.Id);
        dbClient.AddParameter("uid", habbo.Id);
        dbClient.AddParameter("rid", 0);
        dbClient.AddParameter("x", 0);
        dbClient.AddParameter("y", 0);
        dbClient.AddParameter("z", 0);
        dbClient.AddParameter("wall_pos", "");
        dbClient.AddParameter("rot", 0);
        dbClient.AddParameter("extra_data", extraData);
        dbClient.AddParameter("limited_number", limitedNumber);
        dbClient.AddParameter("limited_stack", limitedStack);
        item.Id = Convert.ToUInt32(dbClient.InsertQuery());
        if (groupId > 0)
        {
            dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
            dbClient.AddParameter("id", item.Id);
            dbClient.AddParameter("gid", groupId);
            dbClient.RunQuery();
        }
        return item;
    }

    public Item CreateSingleItem(ItemDefinition definition, Habbo habbo, string extraData, string displayFlags, uint itemId, uint limitedNumber = 0, uint limitedStack = 0)
    {
        if (definition == null) throw new InvalidOperationException("Data cannot be null.");

        var item = new Item()
        {
            Id = itemId,
            OwnerId = (uint)habbo.Id,
            Definition = definition,
            ExtraData = CreateExtraData(definition, extraData),
            UniqueNumber = limitedNumber,
            UniqueSeries = limitedStack
        }; using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery(
            "INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
        dbClient.AddParameter("id", itemId);
        dbClient.AddParameter("did", definition.Id);
        dbClient.AddParameter("uid", habbo.Id);
        dbClient.AddParameter("rid", 0);
        dbClient.AddParameter("x", 0);
        dbClient.AddParameter("y", 0);
        dbClient.AddParameter("z", 0);
        dbClient.AddParameter("wall_pos", "");
        dbClient.AddParameter("rot", 0);
        dbClient.AddParameter("extra_data", extraData);
        dbClient.AddParameter("limited_number", limitedNumber);
        dbClient.AddParameter("limited_stack", limitedStack);
        dbClient.RunQuery();
        return item;
    }

    public Item CreateGiftItem(ItemDefinition definition, Habbo habbo, string extraData, string displayFlags, int itemId, uint limitedNumber = 0, uint limitedStack = 0)
    {
        if (definition == null) throw new InvalidOperationException("Data cannot be null.");
        var item = new Item()
        {
            OwnerId = (uint)habbo.Id,
            Definition = definition,
            ExtraData = new LegacyDataFormat()
            {
                Data = extraData
            },
            UniqueNumber = limitedNumber,
            UniqueSeries = limitedStack,
        };
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery(
            "INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
        dbClient.AddParameter("id", itemId);
        dbClient.AddParameter("did", definition.Id);
        dbClient.AddParameter("uid", habbo.Id);
        dbClient.AddParameter("rid", 0);
        dbClient.AddParameter("x", 0);
        dbClient.AddParameter("y", 0);
        dbClient.AddParameter("z", 0);
        dbClient.AddParameter("wall_pos", "");
        dbClient.AddParameter("rot", 0);
        dbClient.AddParameter("extra_data", extraData);
        dbClient.AddParameter("limited_number", limitedNumber);
        dbClient.AddParameter("limited_stack", limitedStack);
        dbClient.RunQuery();
        return item;
    }

    public List<Item> CreateMultipleItems(ItemDefinition definition, Habbo habbo, string extraData, int amount, int groupId = 0)
    {
        if (definition == null) throw new InvalidOperationException("Data cannot be null.");
        var items = new List<Item>();
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        for (var i = 0; i < amount; i++)
        {
            dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
            dbClient.AddParameter("did", definition.Id);
            dbClient.AddParameter("uid", habbo.Id);
            dbClient.AddParameter("rid", 0);
            dbClient.AddParameter("x", 0);
            dbClient.AddParameter("y", 0);
            dbClient.AddParameter("z", 0);
            dbClient.AddParameter("wallpos", "");
            dbClient.AddParameter("rot", 0);
            dbClient.AddParameter("flags", extraData);

            var item = new Item()
            {
                Id = Convert.ToUInt32(dbClient.InsertQuery()),
                OwnerId = (uint)habbo.Id,
                Definition = definition,
                ExtraData = CreateExtraData(definition, extraData),
                GroupId = groupId
            };
            if (groupId > 0)
            {
                dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
                dbClient.AddParameter("id", item.Id);
                dbClient.AddParameter("gid", groupId);
                dbClient.RunQuery();
            }
            items.Add(item);
        }
        return items;
    }

    public List<Item> CreateTeleporterItems(ItemDefinition definition, Habbo habbo, int groupId = 0)
    {
        var items = new List<Item>();
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
        dbClient.AddParameter("did", definition.Id);
        dbClient.AddParameter("uid", habbo.Id);
        dbClient.AddParameter("rid", 0);
        dbClient.AddParameter("x", 0);
        dbClient.AddParameter("y", 0);
        dbClient.AddParameter("z", 0);
        dbClient.AddParameter("wallpos", "");
        dbClient.AddParameter("rot", 0);
        dbClient.AddParameter("flags", "");
        var item1Id = Convert.ToUInt32(dbClient.InsertQuery());
        dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
        dbClient.AddParameter("did", definition.Id);
        dbClient.AddParameter("uid", habbo.Id);
        dbClient.AddParameter("rid", 0);
        dbClient.AddParameter("x", 0);
        dbClient.AddParameter("y", 0);
        dbClient.AddParameter("z", 0);
        dbClient.AddParameter("wallpos", "");
        dbClient.AddParameter("rot", 0);
        dbClient.AddParameter("flags", item1Id.ToString());
        var item2Id = Convert.ToUInt32(dbClient.InsertQuery());

        var item1 = new Item()
        {
            Id = item1Id,
            OwnerId = (uint)habbo.Id,
            Definition = definition,
            ExtraData = new LegacyDataFormat(),
            GroupId = groupId
        };
        var item2 = new Item()
        {
            Id = item2Id,
            OwnerId = (uint)habbo.Id,
            Definition = definition,
            ExtraData = new LegacyDataFormat(),
            GroupId = groupId
        };
        dbClient.SetQuery($"INSERT INTO `room_items_tele_links` (`tele_one_id`, `tele_two_id`) VALUES ({item1Id}, {item2Id}), ({item2Id}, {item1Id})");
        dbClient.RunQuery();
        items.Add(item1);
        items.Add(item2);
        return items;
    }

    public void CreateMoodlightData(Item item)
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery("INSERT INTO `room_items_moodlight` (`id`, `enabled`, `current_preset`, `preset_one`, `preset_two`, `preset_three`) VALUES (@id, '0', 1, @preset, @preset, @preset);");
        dbClient.AddParameter("id", item.Id);
        dbClient.AddParameter("preset", "#000000,255,0");
        dbClient.RunQuery();
    }

    public void CreateTonerData(Item item)
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery("INSERT INTO `room_items_toner` (`id`, `data1`, `data2`, `data3`, `enabled`) VALUES (@id, 0, 0, 0, '0')");
        dbClient.AddParameter("id", item.Id);
        dbClient.RunQuery();
    }

    private static IFurniObjectData CreateExtraData(ItemDefinition definition, string data)
    {
        if (definition.InteractionType == InteractionType.Background)
        {
            var mapData = (MapDataFormat)definition.CreateData();
            if (!string.IsNullOrEmpty(data))
                mapData.Store(data);
            return mapData;
        }

        return new LegacyDataFormat { Data = data };
    }
}
