using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.Users.Permissions
{
    internal class LoadUserPermissionsTask : IUserDataLoadingTask
    {
        private readonly IPermissionManager _permissionManager;

        public LoadUserPermissionsTask(IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }

        public Task Load(Habbo habbo)
        {
            habbo.SetPermissions(new(_permissionManager.GetPermissionsForPlayer(habbo), _permissionManager.GetCommandsForPlayer(habbo)));
            return Task.CompletedTask;
        }
    }
}
