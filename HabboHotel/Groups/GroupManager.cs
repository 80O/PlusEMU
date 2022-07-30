using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Groups;

public class GroupManager : IGroupManager
{
    private readonly ILogger<GroupManager> _logger;
    private readonly Dictionary<int, GroupColours> _backgroundColours;
    private readonly List<GroupColours> _baseColours;

    private readonly List<GroupBadgeParts> _bases;

    private readonly object _groupLoadingSync;
    private readonly ConcurrentDictionary<int, Group> _groups;
    private readonly Dictionary<int, GroupColours> _symbolColours;
    private readonly List<GroupBadgeParts> _symbols;

    public GroupManager(ILogger<GroupManager> logger)
    {
        _logger = logger;
        _groupLoadingSync = new object();
        _groups = new ConcurrentDictionary<int, Group>();
        _bases = new List<GroupBadgeParts>();
        _symbols = new List<GroupBadgeParts>();
        _baseColours = new List<GroupColours>();
        _symbolColours = new Dictionary<int, GroupColours>();
        _backgroundColours = new Dictionary<int, GroupColours>();
    }


    public ICollection<GroupBadgeParts> BadgeBases => _bases;

    public ICollection<GroupBadgeParts> BadgeSymbols => _symbols;

    public ICollection<GroupColours> BadgeBaseColours => _baseColours;

    public ICollection<GroupColours> BadgeSymbolColours => _symbolColours.Values;

    public ICollection<GroupColours> BadgeBackColours => _backgroundColours.Values;

    public void Init()
    {
        _bases.Clear();
        _symbols.Clear();
        _baseColours.Clear();
        _symbolColours.Clear();
        _backgroundColours.Clear();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT `id`,`type`,`firstvalue`,`secondvalue` FROM `groups_items` WHERE `enabled` = '1'");
        var groupItems = dbClient.GetTable();
        foreach (DataRow groupItem in groupItems.Rows)
        {
            switch (groupItem["type"].ToString())
            {
                case "base":
                    _bases.Add(new GroupBadgeParts(Convert.ToInt32(groupItem["id"]), groupItem["firstvalue"].ToString(), groupItem["secondvalue"].ToString()));
                    break;
                case "symbol":
                    _symbols.Add(new GroupBadgeParts(Convert.ToInt32(groupItem["id"]), groupItem["firstvalue"].ToString(), groupItem["secondvalue"].ToString()));
                    break;
                case "color":
                    _baseColours.Add(new GroupColours(Convert.ToInt32(groupItem["id"]), groupItem["firstvalue"].ToString()));
                    break;
                case "color2":
                    _symbolColours.Add(Convert.ToInt32(groupItem["id"]), new GroupColours(Convert.ToInt32(groupItem["id"]), groupItem["firstvalue"].ToString()));
                    break;
                case "color3":
                    _backgroundColours.Add(Convert.ToInt32(groupItem["id"]), new GroupColours(Convert.ToInt32(groupItem["id"]), groupItem["firstvalue"].ToString()));
                    break;
            }
        }
    }

    public bool TryGetGroup(int id, out Group group)
    {
        group = null;
        if (_groups.ContainsKey(id))
            return _groups.TryGetValue(id, out group);
        lock (_groupLoadingSync)
        {
            if (_groups.ContainsKey(id))
                return _groups.TryGetValue(id, out group);
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("SELECT * FROM `groups` WHERE `id` = @id LIMIT 1");
            dbClient.AddParameter("id", id);
            var row = dbClient.GetRow();
            if (row != null)
            {
                group = new Group(
                    Convert.ToInt32(row["id"]), Convert.ToString(row["name"]), Convert.ToString(row["desc"]), Convert.ToString(row["badge"]), Convert.ToInt32(row["room_id"]),
                    Convert.ToInt32(row["owner_id"]),
                    Convert.ToInt32(row["created"]), Convert.ToInt32(row["state"]), Convert.ToInt32(row["colour1"]), Convert.ToInt32(row["colour2"]), Convert.ToInt32(row["admindeco"]),
                    Convert.ToInt32(row["forum_enabled"]) == 1);
                _groups.TryAdd(group.Id, group);
                return true;
            }
        }
        return false;
    }

    public bool TryCreateGroup(Habbo player, string name, string description, int roomId, string badge, int colour1, int colour2, out Group group)
    {
        group = new Group(0, name, description, badge, roomId, player.Id, (int)UnixTimestamp.GetNow(), 0, colour1, colour2, 0, false);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(badge))
            return false;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery(
            "INSERT INTO `groups` (`name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`) VALUES (@name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, '0', @colour1, @colour2, '0')");
        dbClient.AddParameter("name", group.Name);
        dbClient.AddParameter("desc", group.Description);
        dbClient.AddParameter("owner", group.CreatorId);
        dbClient.AddParameter("badge", group.Badge);
        dbClient.AddParameter("room", group.RoomId);
        dbClient.AddParameter("colour1", group.Colour1);
        dbClient.AddParameter("colour2", group.Colour2);
        group.Id = Convert.ToInt32(dbClient.InsertQuery());
        group.AddMember(player.Id);
        group.MakeAdmin(player.Id);
        if (!_groups.TryAdd(group.Id, group))
            return false;
        dbClient.SetQuery("UPDATE `rooms` SET `group_id` = @gid WHERE `id` = @rid LIMIT 1");
        dbClient.AddParameter("gid", group.Id);
        dbClient.AddParameter("rid", group.RoomId);
        dbClient.RunQuery();
        dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + roomId + "'");
        return true;
    }

    public string GetColourCode(int id, bool colourOne)
    {
        if (colourOne)
        {
            if (_symbolColours.ContainsKey(id)) return _symbolColours[id].Colour;
        }
        else
        {
            if (_backgroundColours.ContainsKey(id)) return _backgroundColours[id].Colour;
        }
        return "";
    }

    public void DeleteGroup(int id)
    {
        Group group = null;
        if (_groups.ContainsKey(id))
            _groups.TryRemove(id, out group);
        if (group != null) group.Dispose();
    }

    public List<Group> GetGroupsForUser(int userId)
    {
        var groups = new List<Group>();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user");
        dbClient.AddParameter("user", userId);
        var getGroups = dbClient.GetTable();
        if (getGroups != null)
        {
            foreach (DataRow row in getGroups.Rows)
            {
                if (TryGetGroup(Convert.ToInt32(row["id"]), out var group))
                    groups.Add(group);
            }
        }
        return groups;
    }

    public Dictionary<int, string> GetAllBadgesInRoom(Room room)
    {
        var badges = new Dictionary<int, string>();
        foreach (var groupIds in room.GetRoomUserManager().GetRoomUsers().Select(user => user.GetClient()?.GetHabbo()?.GetStats().FavouriteGroupId ?? 0).Where(g => g > 0).Distinct())
        {
            if (!TryGetGroup(groupIds, out var group))
                continue;
            badges.Add(group.Id, group.Badge);
        }
        return badges;
    }
}