using Dapper;
using Plus.Database;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.Utilities;

namespace Plus.HabboHotel.Friends
{
    internal class MessengerDataLoader : IMessengerDataLoader
    {
        private readonly IDatabase _database;

        public MessengerDataLoader(IDatabase database)
        {
            _database = database;
        }

        public async Task<List<MessengerBuddy>> GetBuddiesForUser(int userId)
        {
            using var connection = _database.Connection();
            var query = "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom = '1' ,users.hide_online = '1' as AppearOffline, messenger_friendships.relationship FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id  WHERE messenger_friendships.user_one_id = @userId";
            return (await connection.QueryAsync<MessengerBuddy>(query, new { userId })).ToList();
        }

        public async Task<List<MessengerRequest>> GetRequestsForUser(int userId)
        {
            using var connection = _database.Connection();
            return (await connection.QueryAsync<MessengerRequest>("SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = @userId",
                new
                {
                    userId
                })).ToList();
        }

        public async Task<List<int>> GetOutstandingRequestsForUser(int userId)
        {
            using var connection = _database.Connection();
            return (await connection.QueryAsync<int>("SELECT to_id FROM messenger_requests WHERE from_id = @userId", new { userId })).ToList();
        }

        public async Task<(MessengerBuddy from, MessengerBuddy to)> CreateRelationship(int fromUserId, int toUserId)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("INSERT INTO messenger_friendships (user_one_id, user_two_id) VALUES (@fromUserId, @toUserId), (@toUserId, @fromUserId)", new
            {
                fromUserId,
                toUserId
            });
            var from = await GetBuddy(toUserId, fromUserId);
            var to = await GetBuddy(fromUserId, toUserId);

            return (from!, to!);
        }

        public async Task<MessengerBuddy> CreateBuddy(int userId)
        {
            using var connection = _database.Connection();
            var buddy = await connection.QuerySingleAsync<MessengerBuddy>("SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom = '1' ,users.hide_online = '1' as AppearOffline FROM users WHERE id = @userId", new { userId });
            return buddy;
        }

        public async Task<MessengerBuddy?> GetBuddy(int userId, int friendId)
        {
            using var connection = _database.Connection();
            var buddy = await connection.QuerySingleOrDefaultAsync<MessengerBuddy>("SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom = '1' ,users.hide_online = '1' as AppearOffline, messenger_friendships.relationship FROM users INNER JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE users.id = @friendId AND messenger_friendships.user_one_id = @userId", new { userId, friendId });
            return buddy;
        }

        public void BroadcastStatusUpdate(Habbo habbo, MessengerEventTypes eventType, string value)
        {
            foreach (var client in habbo.GetMessenger().Friends.Keys.Select(f => PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(f)))
            {
                if (client == null) continue;
                var messenger = client.GetHabbo().GetMessenger();
                if (!messenger.Friends.TryGetValue(habbo.Id, out var buddy)) continue;
                messenger.UpdateFriendStatus(buddy, eventType, value);
            }
        }

        public async Task LogPrivateMessage(int fromId, int toId, string message)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("INSERT INTO chatlogs_console VALUES (NULL, @fromId, @toId, @message, UNIX_TIMESTAMP())", new { fromId, toId, message });
        }

        public async Task LogPrivateOfflineMessage(int fromId, int toId, string message)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("INSERT INTO `messenger_offline_messages` (`to_id`, `from_id`, `message`, `timestamp`) VALUES (@toId, @fromId, @message, UNIX_TIMESTAMP())", new { toId, fromId, message });
        }

        public async Task<Dictionary<int, List<(string Message, int SecondsAgo)>>> GetAndDeleteOfflineMessages(int userId)
        {
            using var connection = _database.Connection();
            var messages = (await connection.QueryAsync<(int, string, int)>("SELECT from_id, message, timestamp FROM messenger_offline_messages WHERE to_id = @userId", new { userId })).GroupBy(r => r.Item1).ToDictionary(r => r.Key, r => r.OrderBy(r => r.Item3).Select(g => (g.Item2, (int)(PlusEnvironment.Now() - g.Item3))).ToList());
            await connection.ExecuteAsync("DELETE FROM messenger_offline_messages WHERE to_id = @userId", new { userId });
            return messages;
        }

        public async Task<int> GetFriendCount(int userId)
        {
            using var connection = _database.Connection();
            return await connection.ExecuteScalarAsync<int>("SELECT count(0) FROM messenger_friendships WHERE user_one_id = @userid OR user_two_id = @userid", new { userid = userId });
        }

        public async Task DeleteFriendship(int userOneId, int userTwoId)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("DELETE FROM messenger_friendships WHERE (user_one_id = @userOneId AND user_two_id = @userTwoId) OR (user_one_id = @userTwoId AND user_two_id = @userOneId)", new { userOneId, userTwoId });
        }

        public async Task SetRelationship(int userOneId, int userTwoId, int relationship)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("UPDATE messenger_friendships SET relationship = @relationship WHERE user_one_id = @userOneId AND user_two_id = @userTwoId", new { userOneId, userTwoId, relationship });
        }

        public async Task RegisterFriendRequest(int fromUserId, int toUserId)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("INSERT INTO messenger_requests (from_id, to_id) VALUES (@fromUserId, @toUserId)", new { fromUserId, toUserId });
        }

        public async Task DeleteFriendRequest(int fromUserId, int toUserId)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("DELETE FROM messenger_requests WHERE (from_id = @fromUserId AND to_id = @toUserId) OR (from_id = @toUserId AND to_id = @toUserId)", new { fromUserId, toUserId });
        }

        public async Task<(int userId, bool blockFriendRequests)> CanReceiveFriendRequests(string name)
        {
            using var connection = _database.Connection();
            var (userId, blocked) = await connection.QuerySingleOrDefaultAsync<(int, string)>("SELECT `id`,`block_newfriends` FROM `users` WHERE `username` = @name LIMIT 1", new { name });
            return (userId, ConvertExtensions.EnumToBool(blocked));
        }
    }
}
