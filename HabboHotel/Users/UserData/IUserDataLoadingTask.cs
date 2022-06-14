using Plus.Utilities.DependencyInjection;

namespace Plus.HabboHotel.Users.UserData
{
    [Transient]
    public interface IUserDataLoadingTask
    {
        /// <summary>
        /// Decorate the Habbo object.
        /// UserId as well as username is guaranteed to be set.
        /// </summary>
        /// <param name="habbo"></param>
        /// <returns></returns>
        Task Load(Habbo habbo);
    }
}