using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions;

internal class FurniHasNoFurniBox : IWiredItem
{
    public FurniHasNoFurniBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }

    public Item Item { get; set; }

    public WiredBoxType Type => WiredBoxType.ConditionFurniHasNoFurni;

    public ConcurrentDictionary<int, Item> SetItems { get; set; }

    public string StringData { get; set; }

    public bool BoolData { get; set; }

    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var unknown2 = packet.PopString();
        if (SetItems.Count > 0)
            SetItems.Clear();
        var furniCount = packet.PopInt();
        for (var i = 0; i < furniCount; i++)
        {
            var selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
            if (selectedItem != null)
                SetItems.TryAdd(selectedItem.Id, selectedItem);
        }
    }

    public bool Execute(params object[] @params)
    {
        foreach (var item in SetItems.Values.ToList())
        {
            if (item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(item))
                continue;
            var noFurni = false;
            var items = Instance.GetGameMap().GetAllRoomItemForSquare(item.GetX, item.GetY);
            if (items.Count == 0)
                noFurni = true;
            if (!noFurni)
                return false;
        }
        return true;
    }
}