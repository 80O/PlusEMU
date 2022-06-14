using Plus.Utilities;
using System.Collections.Concurrent;
using System.Data;

namespace Plus.HabboHotel.Rooms.Instance;

public class BansComponent
{
    /// <summary>
    /// The bans collection for storing them for this room.
    /// </summary>
    private ConcurrentDictionary<int, double> _bans;
    /// <summary>
    /// The RoomInstance that created this BanComponent.
    /// </summary>
    private Room _instance;

    /// <summary>
    /// Create the BanComponent for the RoomInstance.
    /// </summary>
    /// <param name="instance">The instance that created this component.</param>
    public BansComponent(Room instance)
    {
        if (instance == null)
            return;
        _instance = instance;
        _bans = new ConcurrentDictionary<int, double>();
        DataTable getBans = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `user_id`, `expire` FROM `room_bans` WHERE `room_id` = " + _instance.Id + " AND `expire` > UNIX_TIMESTAMP();");
        getBans = dbClient.GetTable();
        if (getBans != null)
            foreach (DataRow row in getBans.Rows)
                _bans.TryAdd(Convert.ToInt32(row["user_id"]), Convert.ToDouble(row["expire"]));
    }

    public int Count => _bans.Count;

    public void Ban(RoomUser avatar, double time)
    {
        if (avatar == null || _instance.CheckRights(avatar.GetClient(), true) || IsBanned(avatar.UserId))
            return;
        var banTime = UnixTimestamp.GetNow() + time;
        if (!_bans.TryAdd(avatar.UserId, banTime))
            _bans[avatar.UserId] = banTime;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("REPLACE INTO `room_bans` (`user_id`,`room_id`,`expire`) VALUES (@uid, @rid, @expire);");
            dbClient.AddParameter("rid", _instance.Id);
            dbClient.AddParameter("uid", avatar.UserId);
            dbClient.AddParameter("expire", banTime);
            dbClient.RunQuery();
        }
        _instance.GetRoomUserManager().RemoveUserFromRoom(avatar.GetClient(), true, true);
    }

    public bool IsBanned(int userId)
    {
        if (!_bans.ContainsKey(userId))
            return false;
        var banTime = _bans[userId] - UnixTimestamp.GetNow();
        if (banTime <= 0)
        {
            double time;
            _bans.TryRemove(userId, out time);
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
            dbClient.AddParameter("rid", _instance.Id);
            dbClient.AddParameter("uid", userId);
            dbClient.RunQuery();
            return false;
        }
        return true;
    }

    public bool Unban(int userId)
    {
        if (!_bans.ContainsKey(userId))
            return false;
        if (_bans.TryRemove(userId, out var time))
        {
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
            dbClient.AddParameter("rid", _instance.Id);
            dbClient.AddParameter("uid", userId);
            dbClient.RunQuery();
            return true;
        }
        return false;
    }

    public List<int> BannedUsers()
    {
        DataTable getBans = null;
        var bans = new List<int>();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `user_id` FROM `room_bans` WHERE `room_id` = '" + _instance.Id + "' AND `expire` > UNIX_TIMESTAMP();");
        getBans = dbClient.GetTable();
        if (getBans != null)
        {
            foreach (DataRow row in getBans.Rows)
            {
                if (!bans.Contains(Convert.ToInt32(row["user_id"])))
                    bans.Add(Convert.ToInt32(row["user_id"]));
            }
        }
        return bans;
    }

    public void Cleanup()
    {
        _bans.Clear();
        _instance = null;
        _bans = null;
    }
}