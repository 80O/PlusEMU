using Plus.HabboHotel.Achievements;

namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements;

internal class BadgeDefinitionsComposer : ServerPacket
{
    public BadgeDefinitionsComposer(Dictionary<string, Achievement> achievements)
        : base(ServerPacketHeader.BadgeDefinitionsMessageComposer)
    {
        WriteInteger(achievements.Count);
        foreach (var achievement in achievements.Values)
        {
            WriteString(achievement.GroupName.Replace("ACH_", ""));
            WriteInteger(achievement.Levels.Count);
            foreach (var level in achievement.Levels.Values)
            {
                WriteInteger(level.Level);
                WriteInteger(level.Requirement);
            }
        }
    }
}