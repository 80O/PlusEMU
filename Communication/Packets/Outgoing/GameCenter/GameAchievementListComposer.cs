using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.GameCenter;

internal class GameAchievementListComposer : ServerPacket
{
    public GameAchievementListComposer(GameClient session, ICollection<Achievement> achievements, int gameId)
        : base(ServerPacketHeader.GameAchievementListMessageComposer)
    {
        WriteInteger(gameId);
        WriteInteger(achievements.Count);
        foreach (var ach in achievements.ToList())
        {
            var userData = session.GetHabbo().GetAchievementData(ach.GroupName);
            var targetLevel = userData != null ? userData.Level + 1 : 1;
            var targetLevelData = ach.Levels[targetLevel];
            WriteInteger(ach.Id); // ach id
            WriteInteger(targetLevel); // target level
            WriteString(ach.GroupName + targetLevel); // badge
            WriteInteger(targetLevelData.Requirement); // requirement
            WriteInteger(targetLevelData.Requirement); // requirement
            WriteInteger(targetLevelData.RewardPixels); // pixels
            WriteInteger(0); // ach score
            WriteInteger(userData?.Progress ?? 0); // Current progress
            WriteBoolean(userData != null ? userData.Level >= ach.Levels.Count : false); // Set 100% completed(??)
            WriteString(ach.Category);
            WriteString("basejump");
            WriteInteger(0); // total levels
            WriteInteger(0);
        }
        WriteString("");
    }
}