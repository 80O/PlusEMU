namespace Plus.HabboHotel.Users.Messenger
{
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
}