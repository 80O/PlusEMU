namespace Plus.HabboHotel.Users.Authentication;

[Serializable]
public class IncorrectLoginException : Exception
{
    public IncorrectLoginException(string reason) : base(reason) { }
}