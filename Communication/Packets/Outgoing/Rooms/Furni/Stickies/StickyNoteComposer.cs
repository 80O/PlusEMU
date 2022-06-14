namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Stickies;

internal class StickyNoteComposer : ServerPacket
{
    public StickyNoteComposer(string itemId, string extraData)
        : base(ServerPacketHeader.StickyNoteMessageComposer)
    {
        WriteString(itemId);
        WriteString(extraData);
    }
}