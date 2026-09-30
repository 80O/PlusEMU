using System.Collections.Concurrent;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.DataFormat;

namespace Plus.HabboHotel.Users.Inventory.Furniture;

public enum ItemType
{
    Floor,
    Wall
}

public enum FurniCategory
{
    Default = 1,
    WallPaper,
    Floor,
    Landscape,
    PostIt,
    Poster,
    SoundSet,
    TraxSong,
    Present,
    EcotronBox,
    Trophy,
    CreditFurni,
    PetShampoo,
    PetCustomPart,
    PetCustomPartShampoo,
    PetSaddle,
    GuildFurni,
    GameFurni,
    MonsterplantSeed,
    MonsterplantRevival,
    MonsterplantRebreed,
    MonsterplantFertilize
}

public static class ItemTypeExtensions
{
    public static string ToCharCode(this ItemType category) => category switch
    {
        ItemType.Floor => "S",
        ItemType.Wall => "I",
        _ => throw new ArgumentException("Invalid ItemCategory!")
    };

    public static IFurniObjectData CreateData(this ItemDefinition definition)
    {
        if (definition.InteractionType == InteractionType.Gift) return new MapDataFormat();
        if (definition.InteractionType == InteractionType.CrackableEgg)
            return new CrackableDataFormat { Target = (uint)Math.Max(definition.Modes, 1) };
        return EmptyDataFormat.Empty;
    }
}

public class InventoryItem
{
    public uint Id { get; set; }
    public uint OwnerId { get; set; }
    public bool IsFloorItem => Definition.Type == ItemType.Floor;
    public bool IsWallItem => Definition.Type == ItemType.Wall;
    public ItemDefinition Definition { get; set; } = null!;
    public IFurniObjectData ExtraData { get; set; } = FurniObjectData.Empty;

    public uint UniqueNumber;
    public uint UniqueSeries;
}

public class FurnitureInventoryComponent
{
    private readonly ConcurrentDictionary<uint, InventoryItem> _floorItems;
    private readonly ConcurrentDictionary<uint, InventoryItem> _wallItems;

    public IReadOnlyDictionary<uint, InventoryItem> Floor => _floorItems;
    public IReadOnlyDictionary<uint, InventoryItem> Wall => _wallItems;

    public FurnitureInventoryComponent(IEnumerable<InventoryItem> floorFurniture, IEnumerable<InventoryItem> wallFurniture)
    {
        _floorItems = new(floorFurniture.ToDictionary(f => f.Id));
        _wallItems = new(wallFurniture.ToDictionary(w => w.Id));
    }

    public IEnumerable<InventoryItem> GetItems => _floorItems.Values.Concat(_wallItems.Values);

    public IEnumerable<InventoryItem> AllItems => _floorItems.Values.Concat(_wallItems.Values);

    public void ClearItems()
    {
        _floorItems.Clear();
        _wallItems.Clear();
    }

    public InventoryItem? GetItem(uint itemId)
    {
        if (_floorItems.TryGetValue(itemId, out var item))
            return item;
        if (_wallItems.TryGetValue(itemId, out item))
            return item;
        return null;
    }

    public bool AddItem(InventoryItem item)
    {
        if (item.IsFloorItem)
            return _floorItems.TryAdd(item.Id, item);
        else if (item.IsWallItem)
            return _wallItems.TryAdd(item.Id, item);
        else
            throw new InvalidOperationException("Item did not match neither floor or wall item");
    }

    public bool HasItem(uint itemId) => _floorItems.ContainsKey(itemId) || _wallItems.ContainsKey(itemId);

    public bool RemoveItem(uint itemId) => _floorItems.TryRemove(itemId, out _) || _wallItems.TryRemove(itemId, out _);
}
