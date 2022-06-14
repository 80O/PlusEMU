namespace Plus.HabboHotel.Users.Messenger
{
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
}