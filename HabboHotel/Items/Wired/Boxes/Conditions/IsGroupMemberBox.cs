using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions;

internal class IsGroupMemberBox : IWiredItem
{
    public IsGroupMemberBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.ConditionIsGroupMember;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var unknown2 = packet.PopString();
    }

    public bool Execute(params object[] @params)
    {
        if (@params.Length == 0)
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        if (Instance.Group == null)
            return false;
        if (!Instance.Group.IsMember(player.Id))
            return false;
        return true;
    }
}