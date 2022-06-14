using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Ambassadors
{
    public interface IAmbassadorsManager
    {
        Task Warn(Habbo ambassador, Habbo target, string message);
    }
}
