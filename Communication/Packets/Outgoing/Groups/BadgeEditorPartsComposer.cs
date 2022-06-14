using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class BadgeEditorPartsComposer : ServerPacket
{
    public BadgeEditorPartsComposer(ICollection<GroupBadgeParts> bases, ICollection<GroupBadgeParts> symbols, ICollection<GroupColours> baseColours, ICollection<GroupColours> symbolColours,
        ICollection<GroupColours> backgroundColours)
        : base(ServerPacketHeader.BadgeEditorPartsMessageComposer)
    {
        WriteInteger(bases.Count);
        foreach (var part in bases)
        {
            WriteInteger(part.Id);
            WriteString(part.AssetOne);
            WriteString(part.AssetTwo);
        }
        WriteInteger(symbols.Count);
        foreach (var part in symbols)
        {
            WriteInteger(part.Id);
            WriteString(part.AssetOne);
            WriteString(part.AssetTwo);
        }
        WriteInteger(baseColours.Count);
        foreach (var colour in baseColours)
        {
            WriteInteger(colour.Id);
            WriteString(colour.Colour);
        }
        WriteInteger(symbolColours.Count);
        foreach (var colour in symbolColours)
        {
            WriteInteger(colour.Id);
            WriteString(colour.Colour);
        }
        WriteInteger(backgroundColours.Count);
        foreach (var colour in backgroundColours)
        {
            WriteInteger(colour.Id);
            WriteString(colour.Colour);
        }
    }
}