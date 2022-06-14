namespace Plus.HabboHotel.Achievements;

public class Achievement
{
    public Dictionary<int, AchievementLevel> Levels;

    public Achievement(int id, string groupName, string category, int gameId)
    {
        Id = id;
        GroupName = groupName;
        Category = category;
        GameId = gameId;
        Levels = new Dictionary<int, AchievementLevel>();
    }

    public int Id { get; }
    public string Category { get; }
    public string GroupName { get; }
    public int GameId { get; }

    public void AddLevel(AchievementLevel level)
    {
        Levels.Add(level.Level, level);
    }
}