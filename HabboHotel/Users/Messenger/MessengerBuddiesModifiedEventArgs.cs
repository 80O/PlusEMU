namespace Plus.HabboHotel.Users.Messenger
{
    public class MessengerBuddiesModifiedEventArgs : EventArgs
    {
        public Dictionary<MessengerBuddy, BuddyModificationType> Changes { get; }
        public MessengerBuddiesModifiedEventArgs(Dictionary<MessengerBuddy, BuddyModificationType> changes)
        {
            Changes = changes;
        }
    }
}