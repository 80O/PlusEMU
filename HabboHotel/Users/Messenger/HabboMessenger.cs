﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Users.Messenger
{
    public enum BuddyModificationType
    {
        Added = 1,
        Updated = 0,
        Removed = -1
    }

    public enum FriendRequestModificationType
    {
        Received,
        Sent,
        Accepted,
        Declined
    }

    public enum FriendRequestError
    {
        NoFriendRequest,
        AlreadyOutstandingFriendRequest
    }

    public enum MessageError
    {
        NotAFriend,
        EmptyMessage,
        Flooding
    }

    public class MessengerBuddyModifiedEventArgs : EventArgs
    {
        public BuddyModificationType BuddyModificationType { get; }
        public MessengerBuddy Buddy { get; }

        public MessengerBuddyModifiedEventArgs(BuddyModificationType buddyModificationType, MessengerBuddy buddy)
        {
            BuddyModificationType = buddyModificationType;
            Buddy = buddy;
        }
    }

    public class MessengerBuddiesModifiedEventArgs : EventArgs
    {
        public Dictionary<MessengerBuddy, BuddyModificationType> Changes { get; }
        public MessengerBuddiesModifiedEventArgs(Dictionary<MessengerBuddy, BuddyModificationType> changes)
        {
            Changes = changes;
        }
    }

    public class FriendRequestModifiedEventArgs : EventArgs
    {
        public FriendRequestModificationType FriendRequestModificationType { get; }
        public MessengerRequest Request { get; }

        public FriendRequestModifiedEventArgs(FriendRequestModificationType friendRequestModificationType, MessengerRequest request)
        {
            FriendRequestModificationType = friendRequestModificationType;
            Request = request;
        }
    }

    public class MessengerMessageEventArgs : EventArgs
    {
        public MessengerBuddy Friend { get; }
        public string Message { get; }

        public MessengerMessageEventArgs(MessengerBuddy friend, string message)
        {
            Friend = friend;
            Message = message;
        }
    }

    public class FriendStatusUpdatedEventArgs : EventArgs
    {
        public MessengerBuddy Friend { get; }
        public MessengerEventTypes EventType { get; }
        public string Value { get; }

        public FriendStatusUpdatedEventArgs(MessengerBuddy friend, MessengerEventTypes eventType, string value)
        {
            Friend = friend;
            EventType = eventType;
            Value = value;
        }
    }

    public class HabboMessenger
    {
        private readonly ConcurrentDictionary<int, MessengerBuddy> _friends;
        public IReadOnlyDictionary<int, MessengerBuddy> Friends => _friends;
        private readonly ConcurrentDictionary<int, MessengerRequest> _requests;
        public IReadOnlyDictionary<int, MessengerRequest> Requests => _requests;
        private readonly List<int> _outstandingFriendRequests;
        public IReadOnlyCollection<int> OutstandingFriendRequests => _outstandingFriendRequests;

        public event EventHandler<MessengerBuddyModifiedEventArgs>? FriendUpdated;
        public event EventHandler<MessengerBuddiesModifiedEventArgs>? FriendsUpdated;

        public event EventHandler<FriendRequestModifiedEventArgs>? FriendRequestUpdated;
        public event EventHandler<MessengerMessageEventArgs>? RoomInviteReceived;
        public event EventHandler<MessengerMessageEventArgs>? MessageSend;
        public event EventHandler<MessengerMessageEventArgs>? MessageReceived;
        public event EventHandler<FriendStatusUpdatedEventArgs>? FriendStatusUpdated;

        public event EventHandler? StatusUpdated;

        public HabboMessenger(Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests, List<int> outstandingFriendRequests)
        {
            _requests = new(requests);
            _friends = new(friends);
            _outstandingFriendRequests = outstandingFriendRequests;
        }

        public FriendRequestError? AddFriendRequest(MessengerRequest request)
        {
            if (_requests.TryAdd(request.FromId, request))
                FriendRequestUpdated?.Invoke(this, new(FriendRequestModificationType.Received, request));
            return null;
        }

        public FriendRequestError? SendFriendRequest(int toId)
        {
            if (_outstandingFriendRequests.Contains(toId))
                return FriendRequestError.AlreadyOutstandingFriendRequest;
            if (_requests.ContainsKey(toId))
                return AcceptFriendRequest(toId);
            FriendRequestUpdated?.Invoke(this, new(FriendRequestModificationType.Sent, new() { ToId = toId }));
            return null;
        }

        public FriendRequestError? AcceptFriendRequest(int fromId)
        {
            if (!_requests.TryRemove(fromId, out var request))
                return FriendRequestError.NoFriendRequest;
            FriendRequestUpdated?.Invoke(this, new(FriendRequestModificationType.Accepted, request));
            return null;
        }

        public FriendRequestError? DeclineFriendRequest(int fromId)
        {
            if (!_requests.TryRemove(fromId, out var request))
                return FriendRequestError.NoFriendRequest;
            FriendRequestUpdated?.Invoke(this, new(FriendRequestModificationType.Declined, request));
            return null;
        }

        public void ReceiveRoomInvite(MessengerBuddy friend, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            RoomInviteReceived?.Invoke(this, new(friend, message));
        }

        private int _messengerSpamCount = 0;
        private DateTime? _messengerSpamTime = null;
        private DateTime _lastMessage = DateTime.Now;

        private bool IncrementFloodCounter()
        {
            var timeSinceLastMessage = DateTime.Now - _lastMessage;
            if (timeSinceLastMessage > TimeSpan.FromSeconds(5))
                _messengerSpamCount++;
            if (timeSinceLastMessage > TimeSpan.FromSeconds(20))
            {
                _messengerSpamCount = 0;
                return false;
            }

            if (_messengerSpamCount >= 12)
            {
                _messengerSpamTime = DateTime.Now.AddMinutes(1);
                _messengerSpamCount = 0;
                return true;
            }

            if (_messengerSpamTime != null)
            {
                if (!(_messengerSpamTime < DateTime.Now))
                    return true;

                _messengerSpamTime = null;
                return false;
            }
            return false;
        }

        public MessageError? SendMessage(MessengerBuddy friend, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return MessageError.EmptyMessage;
            if (IncrementFloodCounter()) return MessageError.Flooding;
            _lastMessage = DateTime.Now;
            MessageSend?.Invoke(this, new(friend, message));
            return null;
        }

        public void ReceiveMessage(MessengerBuddy friend, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            MessageReceived?.Invoke(this, new(friend, message));
        }

        public void AddFriend(MessengerBuddy friend)
        {
            _friends.TryAdd(friend.Id, friend);
            FriendUpdated?.Invoke(this, new(BuddyModificationType.Added, friend));
            _outstandingFriendRequests.Remove(friend.Id);
        }

        public void UpdateFriend(MessengerBuddy friend) => FriendUpdated?.Invoke(this, new(BuddyModificationType.Updated, friend));

        public void RemoveFriend(MessengerBuddy friend)
        {
            if (_friends.TryRemove(friend.Id, out _))
                FriendUpdated?.Invoke(this, new(BuddyModificationType.Removed, friend));
        }

        public void UpdateFriendStatus(MessengerBuddy friend, MessengerEventTypes eventType, string value) => FriendStatusUpdated?.Invoke(this, new(friend, eventType, value));

        public MessengerBuddy? GetFriend(int userId) => _friends.TryGetValue(userId, out var friend) ? friend : null;

        public bool FriendshipExists(int userId)
        {
            var friend = GetFriend(userId);
            if (friend == null) return false;
            return friend.Relationship > 0;
        }

        public void NotifyChangesToFriends() => StatusUpdated?.Invoke(this, EventArgs.Empty);
    }
}
