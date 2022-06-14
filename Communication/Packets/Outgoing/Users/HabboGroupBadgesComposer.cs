using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Users;

internal class HabboGroupBadgesComposer : ServerPacket
{
    public HabboGroupBadgesComposer(Dictionary<int, string> badges)
        : base(ServerPacketHeader.HabboGroupBadgesMessageComposer)
    {
        WriteInteger(badges.Count);
        foreach (var badge in badges)
        {
            WriteInteger(badge.Key);
            WriteString(badge.Value);
        }
    }

    public HabboGroupBadgesComposer(Group group)
        : base(ServerPacketHeader.HabboGroupBadgesMessageComposer)
    {
        WriteInteger(1); //count
        WriteInteger(group.Id);
        WriteString(group.Badge);
    }
}