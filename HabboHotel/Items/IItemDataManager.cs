namespace Plus.HabboHotel.Items;

public interface IItemDataManager
{
    Task UpdateItemExtradata(Item item);
    void Init();
    ItemDefinition GetItemByName(string name);
    Dictionary<int, uint> Gifts { get; } //<SpriteId, Item>
    Dictionary<uint, ItemDefinition> Items { get; }
}