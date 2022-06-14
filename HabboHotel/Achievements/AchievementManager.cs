using Dapper;
using NLog;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Achievements;

public class AchievementManager : IAchievementManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Achievements.AchievementManager");

    public Dictionary<string, Achievement> Achievements { get; private set; }

    private readonly IDatabase _database;
    private readonly IBadgeManager _badgeManager;

    public AchievementManager(IDatabase database, IBadgeManager badgeManager)
    {
        Achievements = new Dictionary<string, Achievement>();
        _database = database;
        _badgeManager = badgeManager;
    }

    public void Init()
    {
        Achievements = AchievementLevelFactory.GetAchievementLevels();
    }

    public bool ProgressAchievement(GameClient session, string group, int progress, bool fromBeginning = false)
    {
        if (!Achievements.ContainsKey(group) || session == null)
            return false;
        var data = Achievements[group];
        if (data == null) return false;
        var userData = session.GetHabbo().GetAchievementData(group);
        if (userData == null)
        {
            userData = new UserAchievement(group, 0, 0);
            session.GetHabbo().Achievements.TryAdd(group, userData);
        }
        var totalLevels = data.Levels.Count;
        if (userData.Level == totalLevels)
            return false; // done, no more.
        var targetLevel = userData.Level + 1;
        if (targetLevel > totalLevels)
            targetLevel = totalLevels;
        var level = data.Levels[targetLevel];
        int newProgress;
        if (fromBeginning)
            newProgress = progress;
        else
            newProgress = userData.Progress + progress;
        var newLevel = userData.Level;
        var newTarget = newLevel + 1;
        if (newTarget > totalLevels)
            newTarget = totalLevels;
        if (newProgress >= level.Requirement)
        {
            newLevel++;
            newTarget++;
            newProgress = 0;
            if (targetLevel != 1)
                session.GetHabbo().Inventory.Badges.RemoveBadge(Convert.ToString(group + (targetLevel - 1)));
            _badgeManager.GiveBadge(session.GetHabbo(), group + targetLevel).Wait();
            if (newTarget > totalLevels) newTarget = totalLevels;
            session.SendPacket(new AchievementUnlockedComposer(data, targetLevel, level.RewardPoints, level.RewardPixels));
            BroadcastAchievement(session.GetHabbo(), MessengerEventTypes.AchievementUnlocked, group + targetLevel);

            using (var connection = _database.Connection())
            {
                connection.Execute("REPLACE INTO `user_achievements` VALUES (@habboId, @group, @newLevel, @newProgress)",
                    new { habboId = session.GetHabbo().Id, group, newLevel, newProgress });
            }

            userData.Level = newLevel;
            userData.Progress = newProgress;
            session.GetHabbo().Duckets += level.RewardPixels;
            session.GetHabbo().GetStats().AchievementPoints += level.RewardPoints;
            session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, level.RewardPixels));
            session.SendPacket(new AchievementScoreComposer(session.GetHabbo().GetStats().AchievementPoints));
            var newLevelData = data.Levels[newTarget];
            session.SendPacket(new AchievementProgressedComposer(data, newTarget, newLevelData, totalLevels, session.GetHabbo().GetAchievementData(group)));
            return true;
        }
        userData.Level = newLevel;
        userData.Progress = newProgress;

        using (var connection = _database.Connection())
        {
            connection.Execute("REPLACE INTO `user_achievements` VALUES (@habboId, @group, @newLevel, @newProgress)",
                new { habboId = session.GetHabbo().Id, group, newLevel, newProgress });
        }

        session.SendPacket(new AchievementProgressedComposer(data, targetLevel, level, totalLevels, session.GetHabbo().GetAchievementData(group)));
        return false;
    }

    public ICollection<Achievement> GetGameAchievements(int gameId)
    {
        var achievements = new List<Achievement>();
        foreach (var achievement in Achievements.Values.ToList())
        {
            if (achievement.Category == "games" && achievement.GameId == gameId)
                achievements.Add(achievement);
        }
        return achievements;
    }

    public void BroadcastAchievement(Habbo habbo, MessengerEventTypes eventType, string level)
    {
    }
}