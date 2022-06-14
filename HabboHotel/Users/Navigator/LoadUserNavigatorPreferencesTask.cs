using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.Users.Navigator
{
    internal class LoadUserNavigatorPreferencesTask : IUserDataLoadingTask
    {
        private readonly INavigatorManager _navigatorManager;

        public LoadUserNavigatorPreferencesTask(INavigatorManager navigatorManager)
        {
            _navigatorManager = navigatorManager;
        }

        public async Task Load(Habbo habbo)
        {
            habbo.SetNavigatorPreferences(new(new(await _navigatorManager.LoadUserNavigatorPreferences(habbo.Id))));
        }
    }
}
