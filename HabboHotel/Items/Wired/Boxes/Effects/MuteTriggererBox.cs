using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Utilities;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class MuteTriggererBox : IWiredItem
{
    public MuteTriggererBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
        if (SetItems.Count > 0)
            SetItems.Clear();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectMuteTriggerer;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        if (SetItems.Count > 0)
            SetItems.Clear();
        var unknown = packet.PopInt();
        var time = packet.PopInt();
        var message = packet.PopString();
        StringData = time + ";" + message;
    }

    public bool Execute(params object[] @params)
    {
        if (@params.Length != 1)
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        var user = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);
        if (user == null)
            return false;
        if (player.GetPermissions().HasRight("mod_tool") || Instance.OwnerId == player.Id)
        {
            player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, "Wired Mute Exception: Unmutable Player", 0, 0));
            return false;
        }
        var time = StringData != null ? int.Parse(StringData.Split(';')[0]) : 0;
        var message = StringData != null ? StringData.Split(';')[1] : "No message!";
        if (time > 0)
        {
            player.GetClient().SendPacket(new WhisperComposer(user.VirtualId, "Wired Mute: Muted for " + time + "! Message: " + message, 0, 0));
            if (!Instance.MutedUsers.ContainsKey(player.Id))
                Instance.MutedUsers.Add(player.Id, UnixTimestamp.GetNow() + time * 60);
            else
            {
                Instance.MutedUsers.Remove(player.Id);
                Instance.MutedUsers.Add(player.Id, UnixTimestamp.GetNow() + time * 60);
            }
        }
        return true;
    }
}