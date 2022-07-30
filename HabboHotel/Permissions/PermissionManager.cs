using System.Data;
using Microsoft.Extensions.Logging;
using Plus.Database;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Permissions;

public sealed class PermissionManager : IPermissionManager
{
    private readonly IDatabase _database;
    private readonly ILogger<PermissionManager> _logger;

    private readonly Dictionary<string, PermissionCommand> _commands = new();

    private readonly Dictionary<int, List<string>> _permissionGroupRights = new();

    private readonly Dictionary<int, PermissionGroup> _permissionGroups = new();

    private readonly Dictionary<int, Permission> _permissions = new();

    private readonly Dictionary<int, List<string>> _permissionSubscriptionRights = new();

    public PermissionManager(IDatabase database, ILogger<PermissionManager> logger)
    {
        _database = database;
        _logger = logger;
    }

    public void Init()
    {
        _permissions.Clear();
        _commands.Clear();
        _permissionGroups.Clear();
        _permissionGroupRights.Clear();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `permissions`");
            var getPermissions = dbClient.GetTable();
            if (getPermissions != null)
            {
                foreach (DataRow row in getPermissions.Rows)
                    _permissions.Add(Convert.ToInt32(row["id"]), new Permission(Convert.ToInt32(row["id"]), Convert.ToString(row["permission"]), Convert.ToString(row["description"])));
            }
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `permissions_commands`");
            var getCommands = dbClient.GetTable();
            if (getCommands != null)
            {
                foreach (DataRow row in getCommands.Rows)
                    _commands.Add(Convert.ToString(row["command"]), new PermissionCommand(Convert.ToString(row["command"]), Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["subscription_id"])));
            }
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `permissions_groups`");
            var getPermissionGroups = dbClient.GetTable();
            if (getPermissionGroups != null)
            {
                foreach (DataRow row in getPermissionGroups.Rows)
                    _permissionGroups.Add(Convert.ToInt32(row["id"]), new PermissionGroup(Convert.ToString("name"), Convert.ToString("description"), Convert.ToString("badge")));
            }
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `permissions_rights`");
            var getPermissionRights = dbClient.GetTable();
            if (getPermissionRights != null)
            {
                foreach (DataRow row in getPermissionRights.Rows)
                {
                    var groupId = Convert.ToInt32(row["group_id"]);
                    var permissionId = Convert.ToInt32(row["permission_id"]);
                    if (!_permissionGroups.ContainsKey(groupId)) continue; // permission group does not exist
                    Permission permission = null;
                    if (!_permissions.TryGetValue(permissionId, out permission)) continue; // permission does not exist
                    if (_permissionGroupRights.ContainsKey(groupId))
                        _permissionGroupRights[groupId].Add(permission.PermissionName);
                    else
                    {
                        var rightsSet = new List<string>
                        {
                            permission.PermissionName
                        };
                        _permissionGroupRights.Add(groupId, rightsSet);
                    }
                }
            }
        }
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `permissions_subscriptions`");
            var getPermissionSubscriptions = dbClient.GetTable();
            if (getPermissionSubscriptions != null)
            {
                foreach (DataRow row in getPermissionSubscriptions.Rows)
                {
                    var permissionId = Convert.ToInt32(row["permission_id"]);
                    var subscriptionId = Convert.ToInt32(row["subscription_id"]);
                    Permission permission = null;
                    if (!_permissions.TryGetValue(permissionId, out permission))
                        continue; // permission does not exist
                    if (_permissionSubscriptionRights.ContainsKey(subscriptionId))
                        _permissionSubscriptionRights[subscriptionId].Add(permission.PermissionName);
                    else
                    {
                        var rightsSet = new List<string>
                        {
                            permission.PermissionName
                        };
                        _permissionSubscriptionRights.Add(subscriptionId, rightsSet);
                    }
                }
            }
        }
        _logger.LogInformation("Loaded " + _permissions.Count + " permissions.");
        _logger.LogInformation("Loaded " + _permissionGroups.Count + " permissions groups.");
        _logger.LogInformation("Loaded " + _permissionGroupRights.Count + " permissions group rights.");
        _logger.LogInformation("Loaded " + _permissionSubscriptionRights.Count + " permissions subscription rights.");
    }

    public bool TryGetGroup(int id, out PermissionGroup group) => _permissionGroups.TryGetValue(id, out group);

    public List<string> GetPermissionsForPlayer(Habbo player)
    {
        var permissionSet = new List<string>();
        List<string> permRights = null;
        if (_permissionGroupRights.TryGetValue(player.Rank, out permRights)) permissionSet.AddRange(permRights);
        List<string> subscriptionRights = null;
        if (_permissionSubscriptionRights.TryGetValue(player.VipRank, out subscriptionRights)) permissionSet.AddRange(subscriptionRights);
        return permissionSet;
    }

    public List<string> GetCommandsForPlayer(Habbo player)
    {
        return _commands.Where(x => player.Rank >= x.Value.GroupId && player.VipRank >= x.Value.SubscriptionId).Select(x => x.Key).ToList();
    }
}