namespace Plus.HabboHotel.Items;

public static class ItemHopperFinder
{
    public static int GetAHopper(int curRoom)
    {
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        var roomId = 0;
        dbClient.SetQuery("SELECT room_id FROM items_hopper WHERE room_id <> @room ORDER BY room_id ASC LIMIT 1");
        dbClient.AddParameter("room", curRoom);
        roomId = dbClient.GetInteger();
        return roomId;
    }

    public static int GetHopperId(int nextRoom)
    {
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
        dbClient.AddParameter("room", nextRoom);
        var row = dbClient.GetString();
        if (row == null)
            return 0;
        return Convert.ToInt32(row);
    }
}