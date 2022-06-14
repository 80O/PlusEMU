using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Friends
{
    public interface IMessengerDataLoader
    {
        Task<List<MessengerBuddy>> GetBuddiesForUser(int userId);
        Task<List<MessengerRequest>> GetRequestsForUser(int userId);
        Task<List<int>> GetOutstandingRequestsForUser(int userId);
        Task<(MessengerBuddy from, MessengerBuddy to)> CreateRelationship(int fromUserId, int toUserId);
        Task<MessengerBuddy> CreateBuddy(int userId);
        Task<MessengerBuddy?> GetBuddy(int userId, int friendId);
        void BroadcastStatusUpdate(Habbo habbo, MessengerEventTypes eventType, string value);
        Task LogPrivateMessage(int fromId, int toId, string message);
        Task LogPrivateOfflineMessage(int fromId, int toId, string message);
        Task DeleteFriendship(int userOneId, int userTwoId);
        Task SetRelationship(int userOneId, int userTwoId, int relationship);
        Task DeleteFriendRequest(int fromUserId, int toUserId);
        Task RegisterFriendRequest(int fromUserId, int toUserId);
        Task<(int userId, bool blockFriendRequests)> CanReceiveFriendRequests(string name);
        Task<Dictionary<int, List<(string Message, int SecondsAgo)>>> GetAndDeleteOfflineMessages(int userId);
        Task<int> GetFriendCount(int userId);
    }
}