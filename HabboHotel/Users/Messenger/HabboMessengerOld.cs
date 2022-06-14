//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using Plus.Communication.Packets.Outgoing;
//using Plus.Communication.Packets.Outgoing.Messenger;
//using Plus.HabboHotel.GameClients;
//using Plus.HabboHotel.Quests;
//using Plus.Utilities;

//namespace Plus.HabboHotel.Users.Messenger;

//public class HabboMessengerOld
//{
//    private readonly int _userId;

//    private ConcurrentDictionary<int, MessengerBuddy> _friends;
//    private ConcurrentDictionary<int, MessengerRequest> _requests;
//    public bool AppearOffline;

//    public HabboMessengerOld(int userId, Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests)
//    {
//        _userId = userId;
//        _requests = new(requests);
//        _friends = new(friends);
//    }

//    public bool TryGetRequest(int senderId, out MessengerRequest request) => _requests.TryGetValue(senderId, out request);

//    public bool TryGetFriend(int userId, out MessengerBuddy buddy) => _friends.TryGetValue(userId, out buddy);

//    public void ProcessOfflineMessages()
//    {
//        DataTable getMessages = null;
//        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
//        dbClient.SetQuery("SELECT * FROM `messenger_offline_messages` WHERE `to_id` = @id;");
//        dbClient.AddParameter("id", _userId);
//        getMessages = dbClient.GetTable();
//        if (getMessages != null)
//        {
//            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
//            if (client == null)
//                return;
//            foreach (DataRow row in getMessages.Rows)
//                client.SendPacket(new NewConsoleMessageComposer(Convert.ToInt32(row["from_id"]), Convert.ToString(row["message"]), (int)(UnixTimestamp.GetNow() - Convert.ToInt32(row["timestamp"]))));
//            dbClient.SetQuery("DELETE FROM `messenger_offline_messages` WHERE `to_id` = @id");
//            dbClient.AddParameter("id", _userId);
//            dbClient.RunQuery();
//        }
//    }

//    public void Destroy()
//    {
//        var onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);
//        foreach (var client in onlineUsers)
//        {
//            if (client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
//                continue;
//            client.GetHabbo().GetMessenger().UpdateFriend(_userId, null, true);
//        }
//    }

//    public void OnStatusChanged(bool notification)
//    {
//        if (GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().GetMessenger() == null)
//            return;
//        if (_friends == null)
//            return;
//        var onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);
//        if (onlineUsers.Count() == 0)
//            return;
//        foreach (var client in onlineUsers.ToList())
//        {
//            try
//            {
//                if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
//                    continue;
//                client.GetHabbo().GetMessenger().UpdateFriend(_userId, client, true);
//                if (client == null || client.GetHabbo() == null)
//                    continue;
//                UpdateFriend(client.GetHabbo().Id, client, notification);
//            }
//            catch { }
//        }
//    }

//    public void UpdateFriend(int userid, GameClient client, bool notification)
//    {
//        if (_friends.ContainsKey(userid))
//        {
//            _friends[userid].UpdateUser(client);
//            if (notification)
//            {
//                var userclient = GetClient();
//                if (userclient != null)
//                    userclient.SendPacket(SerializeUpdate(_friends[userid]));
//            }
//        }
//    }

//    public void HandleAllRequests()
//    {
//        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//        {
//            dbClient.RunQuery("DELETE FROM messenger_requests WHERE from_id = " + _userId + " OR to_id = " + _userId);
//        }
//        ClearRequests();
//    }

//    public void HandleRequest(int sender)
//    {
//        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//        {
//            dbClient.RunQuery("DELETE FROM messenger_requests WHERE (from_id = " + _userId + " AND to_id = " + sender + ") OR (to_id = " + _userId + " AND from_id = " + sender + ")");
//        }
//        _requests.Remove(sender);
//    }

//    public void CreateFriendship(int friendId)
//    {
//        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//        {
//            dbClient.RunQuery("REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (" + _userId + "," + friendId + ")");
//        }
//        OnNewFriendship(friendId);
//        var user = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);
//        if (user != null && user.GetHabbo().GetMessenger() != null) user.GetHabbo().GetMessenger().OnNewFriendship(_userId);
//        if (user != null)
//            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user, "ACH_FriendListSize", 1);
//        var thisUser = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
//        if (thisUser != null)
//            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(thisUser, "ACH_FriendListSize", 1);
//    }

//    public void DestroyFriendship(int friendId)
//    {
//        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//        {
//            dbClient.RunQuery("DELETE FROM messenger_friendships WHERE (user_one_id = " + _userId + " AND user_two_id = " + friendId + ") OR (user_two_id = " + _userId + " AND user_one_id = " +
//                              friendId + ")");
//        }
//        OnDestroyFriendship(friendId);
//        var user = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);
//        if (user != null && user.GetHabbo().GetMessenger() != null)
//            user.GetHabbo().GetMessenger().OnDestroyFriendship(_userId);
//    }

//    public void OnNewFriendship(int friendId)
//    {
//        var friend = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);
//        MessengerBuddy newFriend;
//        if (friend == null || friend.GetHabbo() == null)
//        {
//            DataRow dRow;
//            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//            {
//                dbClient.SetQuery("SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE `id` = @friendid LIMIT 1");
//                dbClient.AddParameter("friendid", friendId);
//                dRow = dbClient.GetRow();
//            }
//            newFriend = new MessengerBuddy
//            {
//                Id = friendId, Username = Convert.ToString(dRow["username"]), Look = Convert.ToString(dRow["look"]), Motto = Convert.ToString(dRow["motto"]),
//                LastOnline = Convert.ToInt32(dRow["last_online"]), AppearOffline = ConvertExtensions.EnumToBool(dRow["hide_online"].ToString()),
//                HideInRoom = ConvertExtensions.EnumToBool(dRow["hide_inroom"].ToString())
//            };
//        }
//        else
//        {
//            var user = friend.GetHabbo();
//            newFriend = new MessengerBuddy { Id = friendId, Username = user.Username, Look = user.Look, Motto = user.Motto, LastOnline = 0, AppearOffline = user.AppearOffline, HideInRoom = user.AllowPublicRoomStatus };
//            newFriend.UpdateUser(friend);
//        }
//        if (!_friends.ContainsKey(friendId))
//            _friends.Add(friendId, newFriend);
//        GetClient().SendPacket(SerializeUpdate(newFriend));
//    }

//    public bool RequestExists(int requestId)
//    {
//        if (_requests.ContainsKey(requestId))
//            return true;
//        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
//        dbClient.SetQuery(
//            "SELECT user_one_id FROM messenger_friendships WHERE user_one_id = @myID AND user_two_id = @friendID");
//        dbClient.AddParameter("myID", Convert.ToInt32(_userId));
//        dbClient.AddParameter("friendID", Convert.ToInt32(requestId));
//        return dbClient.FindsResult();
//    }

//    public bool FriendshipExists(int friendId) => _friends.ContainsKey(friendId);

//    public void OnDestroyFriendship(int friend)
//    {
//        if (_friends.ContainsKey(friend))
//            _friends.Remove(friend);
//        GetClient().SendPacket(new FriendListUpdateComposer(friend));
//    }

//    public bool RequestBuddy(string userQuery)
//    {
//        int userId;
//        bool hasFqDisabled;
//        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(userQuery);
//        if (client == null)
//        {
//            DataRow row = null;
//            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//            {
//                dbClient.SetQuery("SELECT `id`,`block_newfriends` FROM `users` WHERE `username` = @query LIMIT 1");
//                dbClient.AddParameter("query", userQuery.ToLower());
//                row = dbClient.GetRow();
//            }
//            if (row == null)
//                return false;
//            userId = Convert.ToInt32(row["id"]);
//            hasFqDisabled = ConvertExtensions.EnumToBool(row["block_newfriends"].ToString());
//        }
//        else
//        {
//            userId = client.GetHabbo().Id;
//            hasFqDisabled = client.GetHabbo().AllowFriendRequests;
//        }
//        if (hasFqDisabled)
//        {
//            GetClient().SendPacket(new MessengerErrorComposer(39, 3));
//            return false;
//        }
//        var toId = userId;
//        if (RequestExists(toId))
//            return true;
//        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
//        {
//            dbClient.RunQuery("REPLACE INTO `messenger_requests` (`from_id`,`to_id`) VALUES ('" + _userId + "','" + toId + "')");
//        }
//        PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(GetClient(), QuestType.AddFriends);
//        var toUser = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toId);
//        if (toUser == null || toUser.GetHabbo() == null)
//            return true;
//        var request = new MessengerRequest { ToId = toId, FromId = _userId, Username =PlusEnvironment.GetGame().GetClientManager().GetNameById(_userId)};
//        toUser.GetHabbo().GetMessenger().OnNewRequest(_userId);
//        var thisUser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_userId);
//        if (thisUser != null)
//            toUser.SendPacket(new NewBuddyRequestComposer(thisUser));
//        _requests.Add(toId, request);
//        return true;
//    }

//    public void OnNewRequest(int friendId)
//    {
//        if (!_requests.ContainsKey(friendId))
//            _requests.Add(friendId, new MessengerRequest { ToId = _userId, FromId = friendId, Username = PlusEnvironment.GetGame().GetClientManager().GetNameById(friendId)});
//    }

//    public void SendInstantMessage(int toId, string message)
//    {
//        //if (toId == 0)
//        //    return;
//        //if (GetClient() == null)
//        //    return;
//        //if (GetClient().GetHabbo() == null)
//        //    return;
//        //if (!FriendshipExists(toId))
//        //{
//        //    GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.NotFriends, toId));
//        //    return;
//        //}
//        //if (GetClient().GetHabbo().MessengerSpamCount >= 12)
//        //{
//        //    GetClient().GetHabbo().MessengerSpamTime = UnixTimestamp.GetNow() + 60;
//        //    GetClient().GetHabbo().MessengerSpamCount = 0;
//        //    GetClient().SendNotification("You cannot send a message, you have flooded the console.\n\nYou can send a message in 60 seconds.");
//        //    return;
//        //}
//        //if (GetClient().GetHabbo().MessengerSpamTime > UnixTimestamp.GetNow())
//        //{
//        //    var time = GetClient().GetHabbo().MessengerSpamTime - UnixTimestamp.GetNow();
//        //    GetClient().SendNotification("You cannot send a message, you have flooded the console.\n\nYou can send a message in " + time + " seconds.");
//        //    return;
//        //}
//        //GetClient().GetHabbo().MessengerSpamCount++;
//        //var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toId);
//        //if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
//        //{
//        //    using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
//        //    dbClient.SetQuery("INSERT INTO `messenger_offline_messages` (`to_id`, `from_id`, `message`, `timestamp`) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
//        //    dbClient.AddParameter("tid", toId);
//        //    dbClient.AddParameter("fid", GetClient().GetHabbo().Id);
//        //    dbClient.AddParameter("msg", message);
//        //    dbClient.RunQuery();
//        //    return;
//        //}
//        //if (!client.GetHabbo().AllowConsoleMessages || client.GetHabbo().GetIgnores().IgnoredUserIds().Contains(GetClient().GetHabbo().Id))
//        //{
//        //    GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.FriendBusy, toId));
//        //    return;
//        //}
//        //if (GetClient().GetHabbo().TimeMuted > 0)
//        //{
//        //    GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.YourMuted, toId));
//        //    return;
//        //}
//        //if (client.GetHabbo().TimeMuted > 0) GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.FriendMuted, toId));
//        //if (string.IsNullOrEmpty(message))
//        //    return;
//        //client.SendPacket(new NewConsoleMessageComposer(_userId, message));
//        //LogPm(_userId, toId, message);
//    }

//    public ServerPacket SerializeUpdate(MessengerBuddy friend)
//    {
//        var packet = new ServerPacket(ServerPacketHeader.FriendListUpdateMessageComposer);
//        packet.WriteInteger(0); // category count
//        packet.WriteInteger(1); // number of updates
//        packet.WriteInteger(0); // don't know
//        friend.Serialize(packet, GetClient());
//        return packet;
//    }

//    public void ClearRequests()
//    {
//        _requests.Clear();
//    }

//    private GameClient GetClient() => PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);

//    public ICollection<MessengerRequest> GetRequests() => _requests.Values;

//    public ICollection<MessengerBuddy> GetFriends() => _friends.Values;

//    public void AcceptFriendRequest(MessengerBuddy buddy)
//    {
//        if (!_requests.TryRemove(buddy.Id, out var request)) return;

//    }
//}