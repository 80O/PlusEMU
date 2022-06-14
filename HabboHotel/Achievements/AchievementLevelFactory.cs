using System.Data;

namespace Plus.HabboHotel.Achievements;

public static class AchievementLevelFactory
{
    public static Dictionary<string, Achievement> GetAchievementLevels()
    {
        var achievements = new Dictionary<string, Achievement>();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `id`,`category`,`group_name`,`level`,`reward_pixels`,`reward_points`,`progress_needed`,`game_id` FROM `achievements`");
        var table = dbClient.GetTable();
        if (table != null)
        {
            foreach (DataRow row in table.Rows)
            {
                var id = Convert.ToInt32(row["id"]);
                var category = Convert.ToString(row["category"]);
                var groupName = Convert.ToString(row["group_name"]);
                var rewardPixels = Convert.ToInt32(row["reward_pixels"]);
                var rewardPoints = Convert.ToInt32(row["reward_points"]);
                var progressNeeded = Convert.ToInt32(row["progress_needed"]);
                var level = new AchievementLevel(Convert.ToInt32(row["level"]), rewardPixels, rewardPoints, progressNeeded);
                if (!achievements.ContainsKey(groupName))
                {
                    var achievement = new Achievement(id, groupName, category, Convert.ToInt32(row["game_id"]));
                    achievement.AddLevel(level);
                    achievements.Add(groupName, achievement);
                }
                else
                    achievements[groupName].AddLevel(level);
            }
        }
        return achievements;
    }
}