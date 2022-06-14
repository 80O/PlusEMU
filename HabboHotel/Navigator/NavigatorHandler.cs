using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Data;

namespace Plus.HabboHotel.Navigator;

internal static class NavigatorHandler
{
    public static void Search(ServerPacket packet, SearchResultList result, string query, GameClient session, int limit)
    {
        if (session == null)
            return;
        switch (result.CategoryType)
        {
            default:
            case NavigatorCategoryType.MyHistory:
            case NavigatorCategoryType.Featured:
                packet.WriteInteger(0);
                break;
            case NavigatorCategoryType.Query:
                {
                    if (query.ToLower().StartsWith("owner:"))
                    {
                        if (query.Length > 0)
                        {
                            var userId = 0;
                            DataTable getRooms = null;
                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                if (query.ToLower().StartsWith("owner:"))
                                {
                                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @username LIMIT 1");
                                    dbClient.AddParameter("username", query.Remove(0, 6));
                                    userId = dbClient.GetInteger();
                                    dbClient.SetQuery("SELECT * FROM `rooms` WHERE `owner` = '" + userId + "' and `state` != 'invisible' ORDER BY `users_now` DESC LIMIT 50");
                                    getRooms = dbClient.GetTable();
                                }
                            }
                            var results = new List<RoomData>();
                            if (getRooms != null)
                            {
                                foreach (DataRow row in getRooms.Rows)
                                {
                                    RoomData data = null;
                                    if (!RoomFactory.TryGetData(Convert.ToInt32(row["id"]), out data))
                                        continue;
                                    if (!results.Contains(data))
                                        results.Add(data);
                                }
                                getRooms = null;
                            }
                            packet.WriteInteger(results.Count);
                            foreach (var data in results.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                            results = null;
                        }
                    }
                    else if (query.ToLower().StartsWith("tag:"))
                    {
                        query = query.Remove(0, 4);
                        ICollection<Room> tagMatches = PlusEnvironment.GetGame().GetRoomManager().SearchTaggedRooms(query);
                        packet.WriteInteger(tagMatches.Count);
                        foreach (RoomData data in tagMatches.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                        tagMatches = null;
                    }
                    else if (query.ToLower().StartsWith("group:"))
                    {
                        query = query.Remove(0, 6);
                        ICollection<Room> groupRooms = PlusEnvironment.GetGame().GetRoomManager().SearchGroupRooms(query);
                        packet.WriteInteger(groupRooms.Count);
                        foreach (RoomData data in groupRooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                        groupRooms = null;
                    }
                    else
                    {
                        if (query.Length > 0)
                        {
                            DataTable table = null;
                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery(
                                    "SELECT `id`,`caption`,`description`,`roomtype`,`owner`,`state`,`category`,`users_now`,`users_max`,`model_name`,`score`,`allow_pets`,`allow_pets_eat`,`room_blocking_disabled`,`allow_hidewall`,`password`,`wallpaper`,`floor`,`landscape`,`floorthick`,`wallthick`,`mute_settings`,`kick_settings`,`ban_settings`,`chat_mode`,`chat_speed`,`chat_size`,`trade_settings`,`group_id`,`tags`,`push_enabled`,`pull_enabled`,`enables_enabled`,`respect_notifications_enabled`,`pet_morphs_allowed`,`spush_enabled`,`spull_enabled`,`sale_price` FROM rooms WHERE `caption` LIKE @query ORDER BY `users_now` DESC LIMIT 50");
                                dbClient.AddParameter("query", query + "%");
                                table = dbClient.GetTable();
                            }
                            var results = new List<RoomData>();
                            if (table != null)
                            {
                                foreach (DataRow row in table.Rows)
                                {
                                    if (Convert.ToString(row["state"]) == "invisible")
                                        continue;
                                    RoomData data = null;
                                    if (!RoomFactory.TryGetData(Convert.ToInt32(row["id"]), out data))
                                        continue;
                                    if (!results.Contains(data))
                                        results.Add(data);
                                }
                                table = null;
                            }
                            packet.WriteInteger(results.Count);
                            foreach (var data in results.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                            results = null;
                        }
                    }
                    break;
                }
            case NavigatorCategoryType.Popular:
                {
                    var popularRooms = PlusEnvironment.GetGame().GetRoomManager().GetPopularRooms(-1, limit);
                    packet.WriteInteger(popularRooms.Count);
                    foreach (RoomData data in popularRooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    popularRooms = null;
                    break;
                }
            case NavigatorCategoryType.Recommended:
                {
                    var recommendedRooms = PlusEnvironment.GetGame().GetRoomManager().GetRecommendedRooms(limit);
                    packet.WriteInteger(recommendedRooms.Count);
                    foreach (RoomData data in recommendedRooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    recommendedRooms = null;
                    break;
                }
            case NavigatorCategoryType.Category:
                {
                    var getRoomsByCategory = PlusEnvironment.GetGame().GetRoomManager().GetRoomsByCategory(result.Id, limit);
                    packet.WriteInteger(getRoomsByCategory.Count);
                    foreach (RoomData data in getRoomsByCategory.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    getRoomsByCategory = null;
                    break;
                }
            case NavigatorCategoryType.MyRooms:
                {
                    ICollection<RoomData> rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id).OrderByDescending(x => x.UsersNow).ToList();
                    packet.WriteInteger(rooms.Count);
                    foreach (var data in rooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    break;
                }
            case NavigatorCategoryType.MyFavourites:
                {
                    var favourites = new List<RoomData>();
                    foreach (int id in session.GetHabbo().FavoriteRooms.ToArray())
                    {
                        RoomData data = null;
                        if (!RoomFactory.TryGetData(id, out data))
                            continue;
                        if (!favourites.Contains(data))
                            favourites.Add(data);
                    }
                    packet.WriteInteger(favourites.Count);
                    foreach (var data in favourites.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    favourites = null;
                    break;
                }
            case NavigatorCategoryType.MyGroups:
                {
                    var myGroups = new List<RoomData>();
                    foreach (var group in PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id).ToList())
                    {
                        if (group == null)
                            continue;
                        RoomData data = null;
                        if (!RoomFactory.TryGetData(group.RoomId, out data))
                            continue;
                        if (!myGroups.Contains(data))
                            myGroups.Add(data);
                    }
                    myGroups = myGroups.Take(limit).ToList();
                    packet.WriteInteger(myGroups.Count);
                    foreach (var data in myGroups.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    myGroups = null;
                    break;
                }
            case NavigatorCategoryType.MyFriendsRooms:
                {
                    var roomIds = new List<int>();
                    if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
                        return;
                    foreach (var buddy in session.GetHabbo().GetMessenger().Friends.Values.Where(p => p.InRoom))
                    {
                        if (buddy == null || !buddy.InRoom || buddy.Id == session.GetHabbo().Id)
                            continue;
                        if (!roomIds.Contains(buddy.CurrentRoom.Id))
                            roomIds.Add(buddy.CurrentRoom.Id);
                    }
                    var myFriendsRooms = PlusEnvironment.GetGame().GetRoomManager().GetRoomsByIds(roomIds.ToList());
                    packet.WriteInteger(myFriendsRooms.Count);
                    foreach (RoomData data in myFriendsRooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    myFriendsRooms = null;
                    break;
                }
            case NavigatorCategoryType.MyRights:
                {
                    var myRights = new List<RoomData>();
                    if (session != null)
                    {
                        DataTable getRights = null;
                        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                        dbClient.SetQuery("SELECT `room_id` FROM `room_rights` WHERE `user_id` = @UserId LIMIT @FetchLimit");
                        dbClient.AddParameter("UserId", session.GetHabbo().Id);
                        dbClient.AddParameter("FetchLimit", limit);
                        getRights = dbClient.GetTable();
                        foreach (DataRow row in getRights.Rows)
                        {
                            RoomData data = null;
                            if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out data))
                                continue;
                            if (!myRights.Contains(data))
                                myRights.Add(data);
                        }
                    }
                    packet.WriteInteger(myRights.Count);
                    foreach (var data in myRights.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    myRights = null;
                    break;
                }
            case NavigatorCategoryType.TopPromotions:
                {
                    var getPopularPromotions = PlusEnvironment.GetGame().GetRoomManager().GetOnGoingRoomPromotions(16, limit);
                    packet.WriteInteger(getPopularPromotions.Count);
                    foreach (RoomData data in getPopularPromotions.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    getPopularPromotions = null;
                    break;
                }
            case NavigatorCategoryType.PromotionCategory:
                {
                    var getPromotedRooms = PlusEnvironment.GetGame().GetRoomManager().GetPromotedRooms(result.OrderId, limit);
                    packet.WriteInteger(getPromotedRooms.Count);
                    foreach (RoomData data in getPromotedRooms.ToList()) RoomAppender.WriteRoom(packet, data, data.Promotion);
                    getPromotedRooms = null;
                    break;
                }
        }
    }
}