using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Permissions;

public interface IPermissionManager
{
    void Init();
    bool TryGetGroup(int id, out PermissionGroup group);
    List<string> GetPermissionsForPlayer(Habbo player);
    List<string> GetCommandsForPlayer(Habbo player);
}