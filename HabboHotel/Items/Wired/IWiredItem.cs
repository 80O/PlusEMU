using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired;

public interface IWiredItem
{
    Room Instance { get; set; }
    Item Item { get; set; }
    WiredBoxType Type { get; }
    ConcurrentDictionary<int, Item> SetItems { get; set; }
    string StringData { get; set; }
    bool BoolData { get; set; }
    string ItemsData { get; set; }
    void HandleSave(ClientPacket packet);
    bool Execute(params object[] @params);
}