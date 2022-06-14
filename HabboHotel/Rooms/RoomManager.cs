using NLog;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;
using System.Collections.Concurrent;
using System.Data;

namespace Plus.HabboHotel.Rooms;

public class RoomManager : IRoomManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Rooms.RoomManager");

    private readonly object _roomLoadingSync;

    private readonly Dictionary<string, RoomModel> _roomModels;

    private readonly ConcurrentDictionary<int, Room> _rooms;

    private DateTime _cycleLastExecution;


    public RoomManager()
    {
        _roomModels = new Dictionary<string, RoomModel>();
        _rooms = new ConcurrentDictionary<int, Room>();
        _roomLoadingSync = new object();
    }

    public int Count => _rooms.Count;

    public void OnCycle()
    {
        try
        {
            var sinceLastTime = DateTime.Now - _cycleLastExecution;
            if (sinceLastTime.TotalMilliseconds >= 500)
            {
                _cycleLastExecution = DateTime.Now;
                foreach (var room in _rooms.Values.ToList())
                {
                    if (room.IsCrashed)
                        continue;
                    if (room.ProcessTask == null || room.ProcessTask.IsCompleted)
                    {
                        room.ProcessTask = new Task(room.ProcessRoom);
                        room.ProcessTask.Start();
                        room.IsLagging = 0;
                    }
                    else
                    {
                        room.IsLagging++;
                        if (room.IsLagging >= 30)
                        {
                            room.IsCrashed = true;
                            UnloadRoom(room.Id);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    public void LoadModels()
    {
        if (_roomModels.Count > 0)
            _roomModels.Clear();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '0'");
        var data = dbClient.GetTable();
        if (data == null)
            return;
        foreach (DataRow row in data.Rows)
        {
            var model = Convert.ToString(row["id"]);
            _roomModels.Add(model, new RoomModel(model, Convert.ToInt32(row["door_x"]), Convert.ToInt32(row["door_y"]), (double)row["door_z"], Convert.ToInt32(row["door_dir"]),
                Convert.ToString(row["heightmap"]), ConvertExtensions.EnumToBool(row["club_only"].ToString()), Convert.ToInt32(row["wall_height"]), false));
        }
    }

    public bool LoadModel(string id)
    {
        DataRow row = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = @modelId LIMIT 1");
        dbClient.AddParameter("modelId", id);
        row = dbClient.GetRow();
        if (row == null)
            return false;
        var model = Convert.ToString(row["id"]);
        if (!_roomModels.ContainsKey(model))
        {
            _roomModels.Add(model, new RoomModel(model, Convert.ToInt32(row["door_x"]), Convert.ToInt32(row["door_y"]), Convert.ToDouble(row["door_z"]), Convert.ToInt32(row["door_dir"]),
                Convert.ToString(row["heightmap"]), ConvertExtensions.EnumToBool(row["club_only"].ToString()), Convert.ToInt32(row["wall_height"]), true));
        }
        return true;
    }

    public void ReloadModel(string id)
    {
        if (!_roomModels.ContainsKey(id))
        {
            LoadModel(id);
            return;
        }
        _roomModels.Remove(id);
        LoadModel(id);
    }

    public bool TryGetModel(string id, out RoomModel model)
    {
        if (_roomModels.ContainsKey(id))
        {
            model = _roomModels[id];
            return true;
        }

        // Try to load this model.
        if (LoadModel(id))
        {
            if (TryGetModel(id, out var customModel))
            {
                model = customModel;
                return true;
            }
        }
        model = null;
        return false;
    }

    public void UnloadRoom(int roomId)
    {
        if (_rooms.TryRemove(roomId, out var room)) room.Dispose();
    }

    public bool TryLoadRoom(int roomId, out Room room)
    {
        Room inst = null;
        if (_rooms.TryGetValue(roomId, out inst))
        {
            if (!inst.Unloaded)
            {
                room = inst;
                return true;
            }
            room = null;
            return false;
        }
        lock (_roomLoadingSync)
        {
            if (_rooms.TryGetValue(roomId, out inst))
            {
                if (!inst.Unloaded)
                {
                    room = inst;
                    return true;
                }
                room = null;
                return false;
            }
            if (!RoomFactory.TryGetData(roomId, out var data))
            {
                room = null;
                return false;
            }
            var myInstance = new Room(data);
            if (_rooms.TryAdd(roomId, myInstance))
            {
                room = myInstance;
                return true;
            }
            room = null;
            return false;
        }
    }


    public List<Room> SearchGroupRooms(string query)
    {
        return _rooms.Values.Where(x => x.Group != null && x.Group.Name.ToLower().Contains(query.ToLower()) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(50).ToList();
    }

    public List<Room> SearchTaggedRooms(string query)
    {
        return _rooms.Values.Where(x => x.Tags.Contains(query) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(50).ToList();
    }

    public List<Room> GetPopularRooms(int category, int amount = 50)
    {
        return _rooms.Values.Where(x => x.UsersNow > 0 && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
    }

    public List<Room> GetRecommendedRooms(int amount = 50, int currentRoomId = 0)
    {
        return _rooms.Values.Where(x => x.Id != currentRoomId && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).OrderByDescending(x => x.Score).Take(amount).ToList();
    }

    public List<Room> GetPopularRatedRooms(int amount = 50)
    {
        return _rooms.Values.Where(x => x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Score).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
    }

    public List<Room> GetRoomsByCategory(int category, int amount = 50)
    {
        return _rooms.Values.Where(x => x.Category == category && x.Access != RoomAccess.Invisible && x.UsersNow > 0).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
    }

    public List<Room> GetOnGoingRoomPromotions(int mode, int amount = 50)
    {
        if (mode == 17) return _rooms.Values.Where(x => x.HasActivePromotion && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Promotion.TimestampStarted).Take(amount).ToList();
        return _rooms.Values.Where(x => x.HasActivePromotion && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
    }

    public List<Room> GetPromotedRooms(int categoryId, int amount = 50)
    {
        return _rooms.Values.Where(x => x.HasActivePromotion && x.Promotion.CategoryId == categoryId && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Promotion.TimestampStarted)
            .Take(amount).ToList();
    }

    public List<Room> GetGroupRooms(int amount = 50)
    {
        return _rooms.Values.Where(x => x.Group != null && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Score).Take(amount).ToList();
    }

    public List<Room> GetRoomsByIds(List<int> ids, int amount = 50)
    {
        return _rooms.Values.Where(x => ids.Contains(x.Id) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
    }

    public Room TryGetRandomLoadedRoom()
    {
        return _rooms.Values.Where(x => x.UsersNow > 0 && x.Access != RoomAccess.Invisible && x.UsersNow < x.UsersMax).OrderByDescending(x => x.UsersNow).FirstOrDefault();
    }


    public bool TryGetRoom(int roomId, out Room room) => _rooms.TryGetValue(roomId, out room);

    public RoomData CreateRoom(GameClient session, string name, string description, int category, int maxVisitors, int tradeSettings, RoomModel model, string wallpaper = "0.0", string floor = "0.0",
        string landscape = "0.0", int wallthick = 0, int floorthick = 0)
    {
        if (name.Length < 3)
        {
            session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.creation.name.too_short"));
            return null;
        }
        var roomId = 0;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery(
                "INSERT INTO `rooms` (`roomtype`,`caption`,`description`,`owner`,`model_name`,`category`,`users_max`,`trade_settings`) VALUES ('private',@caption,@description,@UserId,@model,@category,@usersmax,@tradesettings)");
            dbClient.AddParameter("caption", name);
            dbClient.AddParameter("description", description);
            dbClient.AddParameter("UserId", session.GetHabbo().Id);
            dbClient.AddParameter("model", model.Id);
            dbClient.AddParameter("category", category);
            dbClient.AddParameter("usersmax", maxVisitors);
            dbClient.AddParameter("tradesettings", tradeSettings);
            roomId = Convert.ToInt32(dbClient.InsertQuery());
        }
        var data = new RoomData(roomId, name, model.Id, session.GetHabbo().Username, session.GetHabbo().Id, "", 0, "public", "open", 0, maxVisitors, category, description, string.Empty,
            floor, landscape, 1, 1, 0, 0, wallthick, floorthick, wallpaper, 1, 1, 1, 1, 1, 1, 1, 8, tradeSettings, true, true, true, true, true, true, true, 0, 0, true, model);
        return data;
    }

    public ICollection<Room> GetRooms() => _rooms.Values;

    public void Dispose()
    {
        var length = _rooms.Count;
        var i = 0;
        foreach (var room in _rooms.Values.ToList())
        {
            if (room == null)
                continue;
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);
            Console.Clear();
            Log.Info("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + string.Format("{0:0.##}", (double)i / length * 100) + "%");
            i++;
        }
        Log.Info("Done disposing rooms!");
    }
}