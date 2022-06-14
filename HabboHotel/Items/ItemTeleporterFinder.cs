using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items;

public static class ItemTeleporterFinder
{
    public static int GetLinkedTele(int teleId)
    {
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `tele_two_id` FROM `room_items_tele_links` WHERE `tele_one_id` = '" + teleId + "' LIMIT 1");
        var row = dbClient.GetRow();
        if (row == null) return 0;
        return Convert.ToInt32(row[0]);
    }

    public static int GetTeleRoomId(int teleId, Room pRoom)
    {
        if (pRoom.GetRoomItemHandler().GetItem(teleId) != null)
            return pRoom.RoomId;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `room_id` FROM `items` WHERE `id` = " + teleId + " LIMIT 1");
        var row = dbClient.GetRow();
        if (row == null) return 0;
        return Convert.ToInt32(row[0]);
    }

    public static bool IsTeleLinked(int teleId, Room pRoom)
    {
        var linkId = GetLinkedTele(teleId);
        if (linkId == 0) return false;
        var item = pRoom.GetRoomItemHandler().GetItem(linkId);
        if (item != null && item.GetBaseItem().InteractionType == InteractionType.Teleport)
            return true;
        var roomId = GetTeleRoomId(linkId, pRoom);
        if (roomId == 0) return false;
        return true;
    }
}