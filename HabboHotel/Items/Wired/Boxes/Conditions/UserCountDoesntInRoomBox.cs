using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions;

internal class UserCountDoesntInRoomBox : IWiredItem
{
    public UserCountDoesntInRoomBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.ConditionUserCountDoesntInRoom;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var countOne = packet.PopInt();
        var countTwo = packet.PopInt();
        StringData = countOne + ";" + countTwo;
    }

    public bool Execute(params object[] @params)
    {
        if (@params.Length == 0)
            return false;
        if (string.IsNullOrEmpty(StringData))
            return false;
        var countOne = StringData != null ? int.Parse(StringData.Split(';')[0]) : 1;
        var countTwo = StringData != null ? int.Parse(StringData.Split(';')[1]) : 50;
        if (Instance.UserCount >= countOne && Instance.UserCount <= countTwo)
            return false;
        return true;
    }
}