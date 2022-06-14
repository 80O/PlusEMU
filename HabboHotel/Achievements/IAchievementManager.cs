using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Achievements;

public interface IAchievementManager
{
    Dictionary<string, Achievement> Achievements { get; }
    void Init();
    bool ProgressAchievement(GameClient session, string group, int progress, bool fromBeginning = false);
    ICollection<Achievement> GetGameAchievements(int gameId);
}