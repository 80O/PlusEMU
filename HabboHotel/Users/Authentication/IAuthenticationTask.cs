using Plus.Utilities.DependencyInjection;

namespace Plus.HabboHotel.Users.Authentication
{
    [Transient]
    public interface IAuthenticationTask
    {
        /// <summary>
        /// Add additional login checking for a given user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> CanLogin(int userId) => Task.FromResult(true);

        /// <summary>
        /// Raised when the user is logged into the hotel.
        /// </summary>
        /// <param name="habbo"></param>
        /// <returns></returns>
        Task UserLoggedIn(Habbo habbo) => Task.CompletedTask;

        /// <summary>
        /// Raised when the user has been disconnected from the hotel.
        /// </summary>
        /// <param name="habbo"></param>
        /// <returns></returns>
        Task UserLoggedOut(Habbo habbo) => Task.CompletedTask;
    }
}