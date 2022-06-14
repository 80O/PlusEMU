using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Users.Authentication.Tasks
{
    public class DisconnectCurrentOnlineHabboTask : IAuthenticationTask
    {
        private readonly IGameClientManager _gameClientManager;

        public DisconnectCurrentOnlineHabboTask(IGameClientManager gameClientManager)
        {
            _gameClientManager = gameClientManager;
        }

        public Task<bool> CanLogin(int userId)
        {
            var existingSession = _gameClientManager.GetClientByUserId(userId);
            existingSession?.Disconnect();
            return Task.FromResult(true);
        }
    }
}
