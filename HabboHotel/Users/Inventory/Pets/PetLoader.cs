using Plus.HabboHotel.Rooms.AI;
using System.Data;

namespace Plus.HabboHotel.Users.Inventory.Pets;

internal static class PetLoader
{
    public static List<Pet> GetPetsForUser(int userId)
    {
        var pets = new List<Pet>();
        DataTable data = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `user_id` = '" + userId + "' AND `room_id` = '0' AND `ai_type` = 'pet'");
        data = dbClient.GetTable();
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                dbClient.SetQuery(
                    "SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" +
                    Convert.ToInt32(row["id"]) + "' LIMIT 1");
                var mRow = dbClient.GetRow();
                if (mRow != null)
                {
                    pets.Add(new Pet(Convert.ToInt32(row["id"]), Convert.ToInt32(row["user_id"]), Convert.ToInt32(row["room_id"]), Convert.ToString(row["name"]), Convert.ToInt32(mRow["type"]),
                        Convert.ToString(mRow["race"]), Convert.ToString(mRow["color"]),
                        Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]),
                        Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]),
                        Convert.ToDouble(row["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]),
                        Convert.ToString(mRow["gnome_clothing"])));
                }
            }
        }
        return pets;
    }
}