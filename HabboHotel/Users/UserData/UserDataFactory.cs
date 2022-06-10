using Dapper;
using Plus.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.UserData;

public class UserDataFactory : IUserDataFactory
{
    private readonly IDatabase _database;
    private readonly IEnumerable<IUserDataLoadingTask> _userDataLoadingTasks;

    public UserDataFactory(IDatabase database, IEnumerable<IUserDataLoadingTask> userDataLoadingTasks)
    {
        _database = database;
        _userDataLoadingTasks = userDataLoadingTasks;
    }

    public async Task<Habbo?> Create(int userId)
    {
        var habbo = await LoadHabboInfo(userId);

        foreach (var task in _userDataLoadingTasks)
            await task.Load(habbo);

        return habbo;
    }

    public async Task<string> GetUsernameForHabboById(int userId)
    {
        using var connection = _database.Connection();
        return await connection.ExecuteScalarAsync<string>("SELECT username FROM users WHERE id = @userId", new { userId });
    }

    private async Task<Habbo> LoadHabboInfo(int userId)
    {
        using var connection = _database.Connection();
        // TODO: 80O: Load `volume` via IUserDataLoadingTask
        var habbo = await connection.QuerySingleAsync<Habbo>(
            "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points` as Duckets,`home_room`,`block_newfriends` = '1' as AllowFriendRequests,`hide_online` = '1' as AppearOffline,`hide_inroom` = '1' as AllowPublicRoomStatus,`vip`,`account_created`,`vip_points` as Diamonds,`chat_preference` = '1' as `chat_preference`,`focus_preference` = '1' as `focus_preference`, `pets_muted` = '1' as AllowPetSpeech,`bots_muted` = '1' as AllowBotSpeech,`advertising_report_blocked` = '1' as advertising_report_blocked,`last_change` as LastNameChange,`gotw_points`,`ignore_invites` = '1' as AllowMessengerInvites,`time_muted`,`allow_gifts` = '1' as `allow_gifts`,`friend_bar_state`,`disable_forced_effects` = '1' as `disable_forced_effects`,`allow_mimic` = '1' as `allow_mimic`,`rank_vip` as VipRank, `is_ambassador`, `bubble_id` as CustomBubbleId FROM `users` WHERE `id` = @userId LIMIT 1",
            new { userId });
        return habbo;
    }
    //public static UserData GetUserData(string sessionTicket, out byte errorCode)
    //{
    //    int userId;
    //    DataRow dUserInfo = null;
    //    DataTable dAchievements = null;
    //    DataTable dFavouriteRooms = null;
    //    DataTable dBadges = null;
    //    DataTable dFriends = null;
    //    DataTable dRequests = null;
    //    DataTable dQuests = null;
    //    DataTable dRelations = null;
    //    DataRow userInfo = null;
    //    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
    //    {
    //        dbClient.SetQuery(
    //            "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`,`focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip` FROM `users` WHERE `auth_ticket` = @sso LIMIT 1");
    //        dbClient.AddParameter("sso", sessionTicket);
    //        dUserInfo = dbClient.GetRow();
    //        if (dUserInfo == null)
    //        {
    //            errorCode = 1;
    //            return null;
    //        }
    //        userId = Convert.ToInt32(dUserInfo["id"]);
    //        // Done: DisconnectCurrentOnlineHabboTask
    //        //if (PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId) != null)
    //        //{
    //        //    errorCode = 2;
    //        //    PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId).Disconnect();
    //        //    return null;
    //        //}
    //        dbClient.SetQuery("SELECT `group`,`level`,`progress` FROM `user_achievements` WHERE `userid` = '" + userId + "'");
    //        dAchievements = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT room_id FROM user_favorites WHERE `user_id` = '" + userId + "'");
    //        dFavouriteRooms = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT `badge_id`,`badge_slot` FROM user_badges WHERE `user_id` = '" + userId + "'");
    //        dBadges = dbClient.GetTable();
    //        dbClient.SetQuery(
    //            "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
    //            "FROM users " +
    //            "JOIN messenger_friendships " +
    //            "ON users.id = messenger_friendships.user_one_id " +
    //            "WHERE messenger_friendships.user_two_id = " + userId + " " +
    //            "UNION ALL " +
    //            "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
    //            "FROM users " +
    //            "JOIN messenger_friendships " +
    //            "ON users.id = messenger_friendships.user_two_id " +
    //            "WHERE messenger_friendships.user_one_id = " + userId);
    //        dFriends = dbClient.GetTable();
    //        dbClient.SetQuery(
    //            "SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = " +
    //            userId);
    //        dRequests = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT `quest_id`,`progress` FROM user_quests WHERE `user_id` = '" + userId + "'");
    //        dQuests = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT `id`,`user_id`,`target`,`type` FROM `user_relationships` WHERE `user_id` = '" + userId + "'");
    //        dRelations = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
    //        userInfo = dbClient.GetRow();
    //        if (userInfo == null)
    //        {
    //            dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
    //            dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
    //            userInfo = dbClient.GetRow();
    //        }

    //        // Done: Moved to Authenticator as part of login procedure.
    //        //if (!Debugger.IsAttached)
    //        //    dbClient.RunQuery("UPDATE `users` SET `online` = '1', `auth_ticket` = '' WHERE `id` = '" + userId + "' LIMIT 1");
    //    }
    //    var achievements = new ConcurrentDictionary<string, UserAchievement>();
    //    foreach (DataRow dRow in dAchievements.Rows)
    //        achievements.TryAdd(Convert.ToString(dRow["group"]), new UserAchievement(Convert.ToString(dRow["group"]), Convert.ToInt32(dRow["level"]), Convert.ToInt32(dRow["progress"])));
    //    var favouritedRooms = new List<int>();
    //    foreach (DataRow dRow in dFavouriteRooms.Rows) favouritedRooms.Add(Convert.ToInt32(dRow["room_id"]));
    //    var badges = new List<Badge>();
    //    foreach (DataRow dRow in dBadges.Rows) badges.Add(new Badge(Convert.ToString(dRow["badge_id"]), Convert.ToInt32(dRow["badge_slot"])));
    //    var friends = new Dictionary<int, MessengerBuddy>();
    //    foreach (DataRow dRow in dFriends.Rows)
    //    {
    //        var friendId = Convert.ToInt32(dRow["id"]);
    //        var friendName = Convert.ToString(dRow["username"]);
    //        var friendLook = Convert.ToString(dRow["look"]);
    //        var friendMotto = Convert.ToString(dRow["motto"]);
    //        var friendLastOnline = Convert.ToInt32(dRow["last_online"]);
    //        var friendHideOnline = ConvertExtensions.EnumToBool(dRow["hide_online"].ToString());
    //        var friendHideRoom = ConvertExtensions.EnumToBool(dRow["hide_inroom"].ToString());
    //        if (friendId == userId)
    //            continue;
    //        if (!friends.ContainsKey(friendId))
    //            friends.Add(friendId, new MessengerBuddy(friendId, friendName, friendLook, friendMotto, friendLastOnline, friendHideOnline, friendHideRoom));
    //    }
    //    var requests = new Dictionary<int, MessengerRequest>();
    //    foreach (DataRow dRow in dRequests.Rows)
    //    {
    //        var receiverId = Convert.ToInt32(dRow["from_id"]);
    //        var senderId = Convert.ToInt32(dRow["to_id"]);
    //        var requestUsername = Convert.ToString(dRow["username"]);
    //        if (receiverId != userId)
    //        {
    //            if (!requests.ContainsKey(receiverId))
    //                requests.Add(receiverId, new MessengerRequest(userId, receiverId, requestUsername));
    //        }
    //        else
    //        {
    //            if (!requests.ContainsKey(senderId))
    //                requests.Add(senderId, new MessengerRequest(userId, senderId, requestUsername));
    //        }
    //    }
    //    var quests = new Dictionary<int, int>();
    //    foreach (DataRow dRow in dQuests.Rows)
    //    {
    //        var questId = Convert.ToInt32(dRow["quest_id"]);
    //        if (quests.ContainsKey(questId))
    //            quests.Remove(questId);
    //        quests.Add(questId, Convert.ToInt32(dRow["progress"]));
    //    }
    //    var relationships = new Dictionary<int, Relationship>();
    //    foreach (DataRow row in dRelations.Rows)
    //    {
    //        if (friends.ContainsKey(Convert.ToInt32(row[2])))
    //            relationships.Add(Convert.ToInt32(row[2]), new Relationship(Convert.ToInt32(row[0]), Convert.ToInt32(row[2]), Convert.ToInt32(row[3].ToString())));
    //    }
    //    var user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
    //    dUserInfo = null;
    //    dAchievements = null;
    //    dFavouriteRooms = null;
    //    dBadges = null;
    //    dFriends = null;
    //    dRequests = null;
    //    dRelations = null;
    //    errorCode = 0;
    //    return new UserData(userId, achievements, favouritedRooms, badges, friends, requests, quests, user, relationships);
    //}

    //public static UserData GetUserData(int userId)
    //{
    //    DataRow dUserInfo = null;
    //    DataRow userInfo = null;
    //    DataTable dRelations = null;
    //    DataTable dGroups = null;
    //    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
    //    {
    //        dbClient.SetQuery(
    //            "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`, `focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip` FROM `users` WHERE `id` = @id LIMIT 1");
    //        dbClient.AddParameter("id", userId);
    //        dUserInfo = dbClient.GetRow();
    //        PlusEnvironment.GetGame().GetClientManager().LogClonesOut(Convert.ToInt32(userId));
    //        if (dUserInfo == null)
    //            return null;
    //        if (PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId) != null)
    //            return null;
    //        dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
    //        userInfo = dbClient.GetRow();
    //        if (userInfo == null)
    //        {
    //            dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");
    //            dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
    //            userInfo = dbClient.GetRow();
    //        }

    //        dbClient.SetQuery("SELECT `group_id`, `rank` FROM `group_memberships` WHERE `user_id` = @userId");
    //        dbClient.AddParameter("userId", userId);
    //        dGroups = dbClient.GetTable();
    //        dbClient.SetQuery("SELECT `id`,`target`,`type` FROM `user_relationships` WHERE `user_id` = @userId");
    //        dbClient.AddParameter("userId", userId);
    //        dRelations = dbClient.GetTable();
    //    }
    //    var achievements = new ConcurrentDictionary<string, UserAchievement>();
    //    var favouritedRooms = new List<int>();
    //    var badges = new List<Badge>();
    //    var friends = new Dictionary<int, MessengerBuddy>();
    //    var friendRequests = new Dictionary<int, MessengerRequest>();
    //    var quests = new Dictionary<int, int>();
    //    var relationships = new Dictionary<int, Relationship>();
    //    foreach (DataRow row in dRelations.Rows)
    //    {
    //        if (!relationships.ContainsKey(Convert.ToInt32(row["id"])))
    //            relationships.Add(Convert.ToInt32(row["target"]), new Relationship(Convert.ToInt32(row["id"]), Convert.ToInt32(row["target"]), Convert.ToInt32(row["type"].ToString())));
    //    }
    //    var user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
    //    return new UserData(userId, achievements, favouritedRooms, badges, friends, friendRequests, quests, user, relationships);
    //}
}