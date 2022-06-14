using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class ExecuteWiredStacksBox : IWiredItem
{
    public ExecuteWiredStacksBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }

    public Item Item { get; set; }

    public WiredBoxType Type => WiredBoxType.EffectExecuteWiredStacks;

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
        if (@params.Length != 1)
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        foreach (var item in SetItems.Values.ToList())
        {
            if (item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(item) || !item.IsWired)
                continue;
            IWiredItem wiredItem;
            if (Instance.GetWired().TryGet(item.Id, out wiredItem))
            {
                if (wiredItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                    continue;
                var effects = Instance.GetWired().GetEffects(wiredItem);
                if (effects.Count > 0)
                {
                    foreach (var effectItem in effects.ToList())
                    {
                        if (SetItems.ContainsKey(effectItem.Item.Id) && effectItem.Item.Id != item.Id)
                            continue;
                        if (effectItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                            continue;
                        effectItem.Execute(player);
                    }
                }
            }
            else continue;
        }
        return true;
    }
}