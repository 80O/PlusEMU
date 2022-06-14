using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class ToggleFurniBox : IWiredItem, IWiredCycle
{
    private int _delay;

    private long _next;
    private bool _requested;

    public ToggleFurniBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public int TickCount { get; set; }

    public int Delay
    {
        get => _delay;
        set
        {
            _delay = value;
            TickCount = value;
        }
    }

    public bool OnCycle()
    {
        if (SetItems.Count == 0 || !_requested)
            return false;
        var now = DateTime.UtcNow.Ticks;
        if (_next < now)
        {
            foreach (var item in SetItems.Values.ToList())
            {
                if (item == null)
                    continue;
                if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                {
                    Item n = null;
                    SetItems.TryRemove(item.Id, out n);
                    continue;
                }
                item.Interactor.OnWiredTrigger(item);
            }
            _requested = false;
            _next = 0;
            TickCount = Delay;
        }
        return true;
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectToggleFurniState;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        SetItems.Clear();
        var unknown = packet.PopInt();
        var unknown2 = packet.PopString();
        var furniCount = packet.PopInt();
        for (var i = 0; i < furniCount; i++)
        {
            var selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
            if (selectedItem != null)
                SetItems.TryAdd(selectedItem.Id, selectedItem);
        }
        var delay = packet.PopInt();
        Delay = delay;
    }

    public bool Execute(params object[] @params)
    {
        if (_next == 0 || _next < DateTime.UtcNow.Ticks)
            _next = DateTime.UtcNow.Ticks + Delay;
        _requested = true;
        TickCount = Delay;
        return true;
    }
}