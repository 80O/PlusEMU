using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System.Collections;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class TeleportUserBox : IWiredItem, IWiredCycle
{
    private readonly Queue _queue;
    private int _delay;

    public TeleportUserBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
        _queue = new Queue();
        TickCount = Delay;
    }

    public int Delay
    {
        get => _delay;
        set
        {
            _delay = value;
            TickCount = value + 1;
        }
    }

    public int TickCount { get; set; }

    public bool OnCycle()
    {
        if (_queue.Count == 0 || SetItems.Count == 0)
        {
            _queue.Clear();
            TickCount = Delay;
            return true;
        }
        while (_queue.Count > 0)
        {
            var player = (Habbo)_queue.Dequeue();
            if (player == null || player.CurrentRoom != Instance)
                continue;
            TeleportUser(player);
        }
        TickCount = Delay;
        return true;
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectTeleportToFurni;
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
        Delay = packet.PopInt();
    }

    public bool Execute(params object[] @params)
    {
        if (@params == null || @params.Length == 0)
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        if (player.Effects() != null)
            player.Effects().ApplyEffect(4);
        _queue.Enqueue(player);
        return true;
    }

    private void TeleportUser(Habbo player)
    {
        if (player == null)
            return;
        var room = player.CurrentRoom;
        if (room == null)
            return;
        var user = player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
        if (user == null)
            return;
        if (player.IsTeleporting || player.IsHopping || player.TeleporterId != 0)
            return;
        var items = SetItems.Values.ToList();
        items = items.OrderBy(x => Random.Shared.Next()).ToList();
        if (items.Count == 0)
            return;
        var item = items.First();
        if (item == null)
            return;
        if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
        {
            SetItems.TryRemove(item.Id, out item);
            if (items.Contains(item))
                items.Remove(item);
            if (SetItems.Count == 0 || items.Count == 0)
                return;
            item = items.First();
            if (item == null)
                return;
        }
        if (room.GetGameMap() == null)
            return;
        room.GetGameMap().TeleportToItem(user, item);
        room.GetRoomUserManager().UpdateUserStatusses();
        if (player.Effects() != null)
            player.Effects().ApplyEffect(0);
    }
}