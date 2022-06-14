using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class SetRollerSpeedBox : IWiredItem
{
    public SetRollerSpeedBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
        if (SetItems.Count > 0)
            SetItems.Clear();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectSetRollerSpeed;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        if (SetItems.Count > 0)
            SetItems.Clear();
        var unknown = packet.PopInt();
        var message = packet.PopString();
        StringData = message;
        int speed;
        if (!int.TryParse(StringData, out speed)) StringData = "";
    }

    public bool Execute(params object[] @params)
    {
        int speed;
        if (int.TryParse(StringData, out speed)) Instance.GetRoomItemHandler().SetSpeed(speed);
        return true;
    }
}