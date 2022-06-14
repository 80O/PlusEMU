using Plus.HabboHotel.Users.Authentication;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.Moderation
{
    internal class BanLoginCheckTask : IAuthenticationTask
    {
        private readonly IModerationManager _moderationManager;
        private readonly IUserDataFactory _userDataFactory;

        public BanLoginCheckTask(IModerationManager moderationManager, IUserDataFactory userDataFactory)
        {
            _moderationManager = moderationManager;
            _userDataFactory = userDataFactory;
        }

        public async Task<bool> CanLogin(int userId)
        {
            var username = await _userDataFactory.GetUsernameForHabboById(userId);
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (_moderationManager.IsBanned(username, out _)) return false;
            if (_moderationManager.UsernameBanCheck(username)) return false;
            return true;
        }
    }
}
