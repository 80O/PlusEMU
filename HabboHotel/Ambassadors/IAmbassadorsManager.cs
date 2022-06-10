using Plus.HabboHotel.Users;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Ambassadors
{
    public interface IAmbassadorsManager
    {
        Task Warn(Habbo ambassador, Habbo target, string message);
    }
}
