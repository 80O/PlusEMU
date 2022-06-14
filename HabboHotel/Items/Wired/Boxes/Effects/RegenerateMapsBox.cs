using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class RegenerateMapsBox : IWiredItem
{
    public RegenerateMapsBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        StringData = "";
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }

    public WiredBoxType Type => WiredBoxType.EffectRegenerateMaps;

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
        if (Instance == null)
            return false;
        var timeSinceRegen = DateTime.Now - Instance.LastRegeneration;
        if (timeSinceRegen.TotalMinutes > 1)
        {
            Instance.GetGameMap().GenerateMaps();
            Instance.LastRegeneration = DateTime.Now;
            return true;
        }
        return false;
    }
}