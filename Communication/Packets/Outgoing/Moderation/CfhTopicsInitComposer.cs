using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class CfhTopicsInitComposer : ServerPacket
{
    public CfhTopicsInitComposer(Dictionary<string, List<ModerationPresetActions>> userActionPresets)
        : base(ServerPacketHeader.CfhTopicsInitMessageComposer)
    {
        WriteInteger(userActionPresets.Count);
        foreach (var cat in userActionPresets.ToList())
        {
            WriteString(cat.Key);
            WriteInteger(cat.Value.Count);
            foreach (var preset in cat.Value.ToList())
            {
                WriteString(preset.Caption);
                WriteInteger(preset.Id);
                WriteString(preset.Type);
            }
        }
    }
}