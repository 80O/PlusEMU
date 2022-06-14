using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Users.Authentication
{
    public interface IAuthenticator
    {
        /// <summary>
        /// Authenticate a <see cref="GameClient"/> by SSO ticket.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="sso"></param>
        /// <returns></returns>
        Task<AuthenticationError?> AuthenticateUsingSSO(GameClient session, string sso);

        /// <summary>
        /// Raised when a Habbo has succesfully logged in.
        /// </summary>
        event EventHandler<HabboEventArgs>? HabboLoggedIn;
    }
}