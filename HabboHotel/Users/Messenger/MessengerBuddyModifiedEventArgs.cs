namespace Plus.HabboHotel.Users.Messenger
{
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
}