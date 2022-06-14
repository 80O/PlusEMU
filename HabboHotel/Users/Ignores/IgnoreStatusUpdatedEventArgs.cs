namespace Plus.HabboHotel.Users.Ignores
{
    public class IgnoreStatusUpdatedEventArgs : EventArgs
    {
        public IgnoreStatusUpdatedEventArgs(int userId)
        {
            UserId = userId;
        }
        public int UserId { get; }
    }
}