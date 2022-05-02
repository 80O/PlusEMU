using Dapper;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Ignores;

public sealed class IgnoresComponent : IIgnoresComponent
{
    private readonly List<int> _ignoredUsers;
    private readonly IDatabase _database;
    private readonly IAchievementManager _achievementManager;

    public IgnoresComponent(IDatabase database)
    {
        _ignoredUsers = new List<int>();
        _database = database;
    }

    public bool Init(Habbo player)
    {
        if (_ignoredUsers.Count > 0)
            return false;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `user_ignores` WHERE `user_id` = @uid;");
        dbClient.AddParameter("uid", player.Id);
        var getIgnores = dbClient.GetTable();
        if (getIgnores != null)
            foreach (DataRow row in getIgnores.Rows)
                _ignoredUsers.Add(Convert.ToInt32(row["ignore_id"]));
        return true;
    }

    public bool TryGet(Habbo userId) => _ignoredUsers.Contains(userId.Id);

    public bool TryAdd(Habbo userId)
    {
        if (_ignoredUsers.Contains(userId.Id))
            return false;
        _ignoredUsers.Add(userId.Id);
        return true;
    }

    public bool TryRemove(Habbo userId) => _ignoredUsers.Remove(userId.Id);

    public ICollection<int> IgnoredUserIds() => _ignoredUsers;

    public void Dispose()
    {
        _ignoredUsers.Clear();
    }

    public async Task IgnoreUser(Habbo uid, Habbo? ignoredid)
    {
        if (!uid.GetClient().GetHabbo().InRoom)
            return;

        var room = uid.GetClient().GetHabbo().CurrentRoom;

        if (room == null)
            return;

        if (ignoredid == null || ignoredid.GetPermissions().HasRight("mod_tools"))
            return;

        if (uid.GetClient().GetHabbo().GetIgnores().TryGet(ignoredid))
            return;

        if (uid.GetClient().GetHabbo().GetIgnores().TryAdd(ignoredid))
        {
            {
                using var connection = _database.Connection();
                await connection.ExecuteAsync(
                "INSERT INTO user_ignores (user_id,ignore_id) VALUES(@uid,@ignoreId)",
                new { uid = uid.Id, ignoreId = ignoredid.Id }
                );
            }
        }
    }

    public async Task UnIgnoreUser(Habbo uid, Habbo? ignoredid)
    {
        if (!uid.GetClient().GetHabbo().InRoom)
            return;

        var room = uid.GetClient().GetHabbo().CurrentRoom;

        if (room == null)
            return;

        if (ignoredid == null)
            return;

        if (!uid.GetClient().GetHabbo().GetIgnores().TryGet(ignoredid))
            return;

        if (uid.GetClient().GetHabbo().GetIgnores().TryRemove(ignoredid))
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync(
            "DELETE FROM user_ignores WHERE user_id = @uid AND ignore_id = @ignoreId",
            new { uid = uid.Id, ignoreId = ignoredid.Id }
            );

        }
            
    }
}