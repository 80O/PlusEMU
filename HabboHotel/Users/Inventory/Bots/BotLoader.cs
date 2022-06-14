using System.Data;

namespace Plus.HabboHotel.Users.Inventory.Bots;

internal class BotLoader
{
    public static List<Bot> GetBotsForUser(int userId)
    {
        var b = new List<Bot>();
        DataTable dBots = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender`FROM `bots` WHERE `user_id` = '" + userId + "' AND `room_id` = '0' AND `ai_type` != 'pet'");
        dBots = dbClient.GetTable();
        if (dBots != null)
        {
            foreach (DataRow dRow in dBots.Rows)
            {
                b.Add(new Bot(Convert.ToInt32(dRow["id"]), Convert.ToInt32(dRow["user_id"]), Convert.ToString(dRow["name"]),
                    Convert.ToString(dRow["motto"]), Convert.ToString(dRow["look"]), Convert.ToString(dRow["gender"])));
            }
        }
        return b;
    }
}