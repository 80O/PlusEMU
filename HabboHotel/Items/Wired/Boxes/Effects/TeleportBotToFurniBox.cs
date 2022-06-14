using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class TeleportBotToFurniBox : IWiredItem
{
    public TeleportBotToFurniBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectTeleportBotToFurniBox;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var botName = packet.PopString();
        if (SetItems.Count > 0)
            SetItems.Clear();
        var furniCount = packet.PopInt();
        for (var i = 0; i < furniCount; i++)
        {
            var selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
            if (selectedItem != null)
                SetItems.TryAdd(selectedItem.Id, selectedItem);
        }
        StringData = botName;
    }

    public bool Execute(params object[] @params)
    {
        if (string.IsNullOrEmpty(StringData))
            return false;
        var user = Instance.GetRoomUserManager().GetBotByName(StringData);
        if (user == null)
            return false;
        var items = SetItems.Values.ToList();
        items = items.OrderBy(x => Random.Shared.Next()).ToList();
        if (items.Count == 0)
            return false;
        var item = items.First();
        if (item == null)
            return false;
        if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
        {
            SetItems.TryRemove(item.Id, out item);
            if (items.Contains(item))
                items.Remove(item);
            if (SetItems.Count == 0 || items.Count == 0)
                return false;
            item = items.First();
            if (item == null)
                return false;
        }
        if (Instance.GetGameMap() == null)
            return false;
        Instance.GetGameMap().TeleportToItem(user, item);
        Instance.GetRoomUserManager().UpdateUserStatusses();
        return true;
    }
}