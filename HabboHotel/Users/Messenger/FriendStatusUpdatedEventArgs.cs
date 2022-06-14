namespace Plus.HabboHotel.Users.Messenger
{
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
}