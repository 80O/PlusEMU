namespace Plus.HabboHotel.Users.Permissions;

/// <summary>
/// Permissions for a specific Player.
/// </summary>
public sealed class PermissionComponent
{
    private readonly List<string> _commands;
    /// <summary>
    /// Permission rights are stored here.
    /// </summary>
    private readonly List<string> _permissions;

    public PermissionComponent(List<string> permissions, List<string> commands)
    {
        _permissions = permissions;
        _commands = commands;
    }

    /// <summary>
    /// Initialize the PermissionComponent.
    /// </summary>
    /// <param name="habbo"></param>
    [Obsolete]
    public bool Init(Habbo habbo)
    {
        return true;
    }

    /// <summary>
    /// Checks if the user has the specified right.
    /// </summary>
    /// <param name="right"></param>
    /// <returns></returns>
    public bool HasRight(string right) => _permissions.Contains(right);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool HasCommand(string command) => _commands.Contains(command);

    /// <summary>
    /// Dispose of the permissions list.
    /// </summary>
    public void Dispose()
    {
        _permissions.Clear();
    }
}