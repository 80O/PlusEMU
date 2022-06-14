using Plus.HabboHotel.Items;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Users.Inventory.Furniture
{
    public class FurnitureInventoryComponent
    {
        private readonly ConcurrentDictionary<int, Item> _floorItems;
        private readonly ConcurrentDictionary<int, Item> _wallItems;

        public IReadOnlyDictionary<int, Item> Floor => _floorItems;
        public IReadOnlyDictionary<int, Item> Wall => _wallItems;

        public FurnitureInventoryComponent(List<Item> floorFurniture, List<Item> wallFurniture)
        {
            _floorItems = new(floorFurniture.ToDictionary(f => f.Id));
            _wallItems = new(wallFurniture.ToDictionary(w => w.Id));
        }

        public IEnumerable<Item> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        public IEnumerable<Item> AllItems => _floorItems.Values.Concat(_wallItems.Values);

        public void ClearItems()
        {
            _floorItems.Clear();
            _wallItems.Clear();
        }

        public Item? GetItem(int itemId)
        {
            if (_floorItems.TryGetValue(itemId, out var item))
                return item;
            if (_wallItems.TryGetValue(itemId, out item))
                return item;
            return null;
        }

        public bool AddItem(Item item)
        {
            if (item.IsFloorItem)
                return _floorItems.TryAdd(item.Id, item);
            else if (item.IsWallItem)
                return _wallItems.TryAdd(item.Id, item);
            else
                throw new InvalidOperationException("Item did not match neither floor or wall item");
        }

        public bool HasItem(int itemId) => _floorItems.ContainsKey(itemId) || _wallItems.ContainsKey(itemId);

        public bool RemoveItem(int itemId) => _floorItems.TryRemove(itemId, out _) || _wallItems.TryRemove(itemId, out _);
    }
}
