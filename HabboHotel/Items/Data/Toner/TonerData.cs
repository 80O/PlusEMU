using System.Data;

namespace Plus.HabboHotel.Items.Data.Toner;

public class TonerData
{
    public int Enabled;
    public int Hue;
    public int ItemId;
    public int Lightness;
    public int Saturation;

    public TonerData(int item)
    {
        ItemId = item;
        DataRow row;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId + " LIMIT 1");
            row = dbClient.GetRow();
        }
        if (row == null)
        {
            //throw new NullReferenceException("No toner data found in the database for " + ItemId);
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.RunQuery("INSERT INTO `room_items_toner` VALUES (" + ItemId + ",'0',0,0,0)");
            dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId + " LIMIT 1");
            row = dbClient.GetRow();
        }
        Enabled = int.Parse(row[0].ToString());
        Hue = Convert.ToInt32(row[1]);
        Saturation = Convert.ToInt32(row[2]);
        Lightness = Convert.ToInt32(row[3]);
    }
}