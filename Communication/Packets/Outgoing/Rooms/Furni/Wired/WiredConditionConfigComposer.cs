using Plus.HabboHotel.Items.Wired;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Wired;

internal class WiredConditionConfigComposer : ServerPacket
{
    public WiredConditionConfigComposer(IWiredItem box)
        : base(ServerPacketHeader.WiredConditionConfigMessageComposer)
    {
        WriteBoolean(false);
        WriteInteger(5);
        WriteInteger(box.SetItems.Count);
        foreach (var item in box.SetItems.Values.ToList()) WriteInteger(item.Id);
        WriteInteger(box.Item.GetBaseItem().SpriteId);
        WriteInteger(box.Item.Id);
        WriteString(box.StringData);
        if (box.Type == WiredBoxType.ConditionMatchStateAndPosition || box.Type == WiredBoxType.ConditionDontMatchStateAndPosition)
        {
            if (string.IsNullOrEmpty(box.StringData))
                box.StringData = "0;0;0";
            WriteInteger(3); //Loop
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[0]) : 0);
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[1]) : 0);
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[2]) : 0);
        }
        else if (box.Type == WiredBoxType.ConditionUserCountInRoom || box.Type == WiredBoxType.ConditionUserCountDoesntInRoom)
        {
            if (string.IsNullOrEmpty(box.StringData))
                box.StringData = "0;0";
            WriteInteger(2); //Loop
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[0]) : 1);
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[1]) : 50);
        }
        if (box.Type == WiredBoxType.ConditionFurniHasNoFurni)
            WriteInteger(1);
        if (box.Type != WiredBoxType.ConditionUserCountInRoom && box.Type != WiredBoxType.ConditionUserCountDoesntInRoom && box.Type != WiredBoxType.ConditionFurniHasNoFurni)
            WriteInteger(0);
        else if (box.Type == WiredBoxType.ConditionFurniHasNoFurni)
        {
            if (string.IsNullOrEmpty(box.StringData))
                box.StringData = "0";
            WriteInteger(box.StringData != null ? int.Parse(box.StringData.Split(';')[0]) : 50);
        }
        WriteInteger(0);
        WriteInteger(WiredBoxTypeUtility.GetWiredId(box.Type));
    }
}