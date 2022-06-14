namespace Plus.HabboHotel.Users.Authentication
{
    public class HabboEventArgs : EventArgs
    {
        public Habbo Habbo { get; }

        public HabboEventArgs(Habbo habbo)
        {
            Habbo = habbo;
        }
    }
}