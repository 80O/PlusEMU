using System.Data;
using Plus.Communication.Packets;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Items.Data.Toner;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Rooms.Games.Banzai;
using Plus.HabboHotel.Rooms.Games.Football;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.Instance;
using Plus.Utilities;
using Plus.Utilities.DependencyInjection;

namespace Plus.HabboHotel.Rooms;

[Scoped]
public interface IRoomComponent
{

}

public class Room : RoomData
{
    public IEnumerable<IRoomComponent> Components { get; }
    private BansComponent _bansComponent;

    private FilterComponent _filterComponent;

    private Dictionary<uint, List<RoomUser>> _tents = new();
    private TradingComponent _tradingComponent;
    private WiredComponent _wiredComponent;
    private BattleBanzai _banzai;
    private Freeze _freeze;
    private GameItemHandler _gameItemHandler;
    private GameManager _gameManager;

    private Gamemap _gamemap;
    private RoomItemHandling _roomItemHandling;

    private RoomUserManager _roomUserManager;
    private Soccer _soccer;

    public bool IsCrashed;
    public DateTime LastRegeneration;
    public DateTime LastTimerReset;
    public bool MDisposed;
    public MoodlightData MoodlightData;

    public Dictionary<int, double> MutedUsers = new();

    public Task ProcessTask;
    public bool RoomMuted;

    public TeamManager Teambanzai;
    public TeamManager Teamfreeze;

    public TonerData TonerData;

    public List<int> UsersWithRights;

    public Room(IEnumerable<IRoomComponent> components)
    {
        Components = components;
        _bansComponent = Components.OfType<BansComponent>().First();
        
        LastRegeneration = DateTime.Now;
    }

    public override void SetData(RoomData data)
    {
        base.SetData(data);

        // Temp
        _roomItemHandling = new(this);
        _roomUserManager = new(this);
        _filterComponent = new(this);
        _wiredComponent = new(this);
        //_bansComponent = new(this);
        _tradingComponent = new(this);
        _gamemap = new(this, data.Model);
        GetRoomItemHandler().LoadFurniture();
        GetGameMap().GenerateMaps();
        LoadPromotions();
        LoadRights();
        LoadFilter();
        InitBots();
        InitPets();
    }

    public int IsLagging { get; set; }
    public bool Unloaded { get; set; }
    public int IdleTime { get; set; }

    public List<string> WordFilterList { get; set; }

    public int UserCount => _roomUserManager.GetRoomUsers().Count;

    public uint RoomId => Id;

    public bool CanTradeInRoom => true;

    public Gamemap GetGameMap() => _gamemap;

    public RoomItemHandling GetRoomItemHandler()
    {
        if (_roomItemHandling == null) _roomItemHandling = new(this);
        return _roomItemHandling;
    }

    public RoomUserManager GetRoomUserManager() => _roomUserManager;

    public Soccer GetSoccer()
    {
        if (_soccer == null)
            _soccer = new(this);
        return _soccer;
    }

    public TeamManager GetTeamManagerForBanzai()
    {
        if (Teambanzai == null)
            Teambanzai = TeamManager.CreateTeam("banzai");
        return Teambanzai;
    }

    public TeamManager GetTeamManagerForFreeze()
    {
        if (Teamfreeze == null)
            Teamfreeze = TeamManager.CreateTeam("freeze");
        return Teamfreeze;
    }

    public BattleBanzai GetBanzai()
    {
        if (_banzai == null)
            _banzai = new(this);
        return _banzai;
    }

    public Freeze GetFreeze()
    {
        if (_freeze == null)
            _freeze = new(this);
        return _freeze;
    }

    public GameManager GetGameManager()
    {
        if (_gameManager == null)
            _gameManager = new(this);
        return _gameManager;
    }

    public GameItemHandler GetGameItemHandler()
    {
        if (_gameItemHandler == null)
            _gameItemHandler = new(this);
        return _gameItemHandler;
    }

    public bool GotSoccer() => _soccer != null;

    public bool GotBanzai() => _banzai != null;

    public bool GotFreeze() => _freeze != null;

    public void ClearTags()
    {
        Tags.Clear();
    }

    public void AddTagRange(List<string> tags)
    {
        Tags.AddRange(tags);
    }

    public void InitBots()
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery(
            $"SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `room_id` = '{RoomId}' AND `ai_type` != 'pet'");
        var data = dbClient.GetTable();
        if (data == null)
            return;
        foreach (DataRow bot in data.Rows)
        {
            dbClient.SetQuery($"SELECT `text` FROM `bots_speech` WHERE `bot_id` = '{Convert.ToInt32(bot["id"])}'");
            var botSpeech = dbClient.GetTable();
            var speeches = new List<RandomSpeech>();
            foreach (DataRow speech in botSpeech.Rows) speeches.Add(new(Convert.ToString(speech["text"]), Convert.ToInt32(bot["id"])));
            _roomUserManager.DeployBot(
                new(Convert.ToInt32(bot["id"]), Convert.ToUInt32(bot["room_id"]), Convert.ToString(bot["ai_type"]), Convert.ToString(bot["walk_mode"]), Convert.ToString(bot["name"]),
                    Convert.ToString(bot["motto"]), Convert.ToString(bot["look"]), int.Parse(bot["x"].ToString()), int.Parse(bot["y"].ToString()), int.Parse(bot["z"].ToString()),
                    int.Parse(bot["rotation"].ToString()), 0, 0, 0, 0, ref speeches, "M", 0, Convert.ToInt32(bot["user_id"].ToString()), Convert.ToBoolean(bot["automatic_chat"]),
                    Convert.ToInt32(bot["speaking_interval"]), ConvertExtensions.EnumToBool(bot["mix_sentences"].ToString()), Convert.ToInt32(bot["chat_bubble"])), null);
        }
    }

    public void InitPets()
    {
        using var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor();
        dbClient.SetQuery($"SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '{RoomId}' AND `ai_type` = 'pet'");
        var data = dbClient.GetTable();
        if (data == null)
            return;
        foreach (DataRow row in data.Rows)
        {
            dbClient.SetQuery(
                $"SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '{row[0]}' LIMIT 1");
            var mRow = dbClient.GetRow();
            if (mRow == null)
                continue;
            var pet = new Pet(Convert.ToInt32(row["id"]), Convert.ToInt32(row["user_id"]), Convert.ToUInt32(row["room_id"]), Convert.ToString(row["name"]), Convert.ToInt32(mRow["type"]),
                Convert.ToString(mRow["race"]),
                Convert.ToString(mRow["color"]), Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]),
                Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]),
                Convert.ToDouble(row["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]),
                Convert.ToString(mRow["gnome_clothing"]));
            var rndSpeechList = new List<RandomSpeech>();
            _roomUserManager.DeployBot(
                new(pet.PetId, RoomId, "pet", "freeroam", pet.Name, "", pet.Look, pet.X, pet.Y, Convert.ToInt32(pet.Z), 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false,
                    0), pet);
        }
    }

    public FilterComponent GetFilter() => _filterComponent;

    public WiredComponent GetWired() => _wiredComponent;

    public BansComponent GetBans() => _bansComponent;

    public TradingComponent GetTrading() => _tradingComponent;

    public void LoadRights()
    {
        UsersWithRights = new();
        if (Group != null)
            return;
        DataTable data = null;
        using (var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
            dbClient.AddParameter("roomid", Id);
            data = dbClient.GetTable();
        }
        if (data != null)
            foreach (DataRow row in data.Rows)
                UsersWithRights.Add(Convert.ToInt32(row["user_id"]));
    }

    private void LoadFilter()
    {
        WordFilterList = new();
        DataTable data = null;
        using (var dbClient = PlusEnvironment.DatabaseManager.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
            dbClient.AddParameter("roomid", Id);
            data = dbClient.GetTable();
        }
        if (data == null)
            return;
        foreach (DataRow row in data.Rows) WordFilterList.Add(Convert.ToString(row["word"]));
    }

    public bool CheckRights(GameClient session) => CheckRights(session, false);

    public bool CheckRights(GameClient session, bool requireOwnership, bool checkForGroups = false)
    {
        try
        {
            if (session == null || session.GetHabbo() == null)
                return false;
            if (session.GetHabbo().Username == OwnerName && Type == "private")
                return true;
            if (session.GetHabbo().Permissions.HasRight("room_any_owner"))
                return true;
            if (!requireOwnership && Type == "private")
            {
                if (session.GetHabbo().Permissions.HasRight("room_any_rights"))
                    return true;
                if (UsersWithRights.Contains(session.GetHabbo().Id))
                    return true;
            }
            if (checkForGroups && Type == "private")
            {
                if (Group == null)
                    return false;
                if (Group.IsAdmin(session.GetHabbo().Id))
                    return true;
                if (Group.AdminOnlyDeco == 0)
                {
                    if (Group.IsAdmin(session.GetHabbo().Id))
                        return true;
                }
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
        return false;
    }

    public void OnUserShoot(RoomUser user, Item ball)
    {
        Func<Item, bool> predicate = null;
        string key = null;
        foreach (var item in GetRoomItemHandler().GetFurniObjects(ball.GetX, ball.GetY).ToList())
        {
            if (item.Definition.ItemName.StartsWith("fball_goal_"))
            {
                key = item.Definition.ItemName.Split(new[] { '_' })[2];
                user.UnIdle();
                user.DanceId = 0;
                PlusEnvironment.Game.AchievementManager.ProgressAchievement(user.GetClient(), "ACH_FootballGoalScored", 1);
                SendPacket(new ActionComposer(user.VirtualId, 1));
            }
        }
        if (key != null)
        {
            if (predicate == null) predicate = p => p.Definition.ItemName == $"fball_score_{key}";
            foreach (var item2 in GetRoomItemHandler().GetFloor.Where(predicate).ToList())
            {
                if (item2.Definition.ItemName == $"fball_score_{key}")
                {
                    if (!string.IsNullOrEmpty(item2.LegacyDataString))
                        item2.LegacyDataString = (Convert.ToInt32(item2.LegacyDataString) + 1).ToString();
                    else
                        item2.LegacyDataString = "1";
                    item2.UpdateState();
                }
            }
        }
    }

    public void ProcessRoom()
    {
        if (IsCrashed || MDisposed)
            return;
        try
        {
            if (GetRoomUserManager().GetRoomUsers().Count == 0)
                IdleTime++;
            else if (IdleTime > 0)
                IdleTime = 0;
            if (HasActivePromotion && Promotion.HasExpired) EndPromotion();
            if (IdleTime >= 60 && !HasActivePromotion)
            {
                PlusEnvironment.Game.RoomManager.UnloadRoom(Id);
                return;
            }
            try
            {
                GetRoomItemHandler().OnCycle();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            try
            {
                GetRoomUserManager().OnCycle();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            try
            {
                GetRoomUserManager().SerializeStatusUpdates();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            try
            {
                if (_gameItemHandler != null)
                    _gameItemHandler.OnCycle();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            try
            {
                GetWired().OnCycle();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
            OnRoomCrash(e);
        }
    }

    private void OnRoomCrash(Exception e)
    {
        try
        {
            foreach (var user in _roomUserManager.GetRoomUsers().ToList())
            {
                if (user == null || user.GetClient() == null)
                    continue;
                user.GetClient().SendNotification("Sorry, it appears that room has crashed!"); //Unhandled exception in room: " + e);
                try
                {
                    GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true);
                }
                catch (Exception e2)
                {
                    ExceptionLogger.LogException(e2);
                }
            }
        }
        catch (Exception e3)
        {
            ExceptionLogger.LogException(e3);
        }
        IsCrashed = true;
        PlusEnvironment.Game.RoomManager.UnloadRoom(Id);
    }


    public bool CheckMute(GameClient session)
    {
        if (MutedUsers.ContainsKey(session.GetHabbo().Id))
        {
            if (MutedUsers[session.GetHabbo().Id] < UnixTimestamp.GetNow())
                MutedUsers.Remove(session.GetHabbo().Id);
            else
                return true;
        }
        if (session.GetHabbo().TimeMuted > 0 || RoomMuted && session.GetHabbo().Username != OwnerName)
            return true;
        return false;
    }

    public void SendObjects(GameClient session)
    {
        session.Send(new HeightMapComposer(GetGameMap().Model.Heightmap));
        session.Send(new FloorHeightMapComposer(GetGameMap().Model.GetRelativeHeightmap(), GetGameMap().StaticModel.WallHeight));
        foreach (var user in _roomUserManager.GetUserList().ToList())
        {
            if (user == null)
                continue;
            session.Send(new UsersComposer(user));
            if (user.IsBot && user.BotData.DanceId > 0)
                session.Send(new DanceComposer(user, user.BotData.DanceId));
            else if (!user.IsBot && !user.IsPet && user.IsDancing)
                session.Send(new DanceComposer(user, user.DanceId));
            if (user.IsAsleep)
                session.Send(new SleepComposer(user, true));
            if (user.CarryItemId > 0 && user.CarryTimer > 0)
                session.Send(new CarryObjectComposer(user.VirtualId, user.CarryItemId));
            if (!user.IsBot && !user.IsPet && user.CurrentEffect > 0)
                session.Send(new AvatarEffectComposer(user.VirtualId, user.CurrentEffect));
        }
        session.Send(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
        session.Send(new ObjectsComposer(GetRoomItemHandler().GetFloor.ToArray(), this));
        session.Send(new ItemsComposer(GetRoomItemHandler().GetWall.ToArray(), this));
    }

    public void AddTent(uint tentId)
    {
        if (_tents.ContainsKey(tentId))
            _tents.Remove(tentId);
        _tents.Add(tentId, new());
    }

    public void RemoveTent(uint tentId)
    {
        if (!_tents.ContainsKey(tentId))
            return;
        var users = _tents[tentId];
        foreach (var user in users.ToList())
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                continue;
            user.GetClient().GetHabbo().TentId = 0;
        }
        if (_tents.ContainsKey(tentId))
            _tents.Remove(tentId);
    }

    public void AddUserToTent(uint tentId, RoomUser user)
    {
        if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
        {
            if (!_tents.ContainsKey(tentId))
                _tents.Add(tentId, new());
            if (!_tents[tentId].Contains(user))
                _tents[tentId].Add(user);
            user.GetClient().GetHabbo().TentId = tentId;
        }
    }

    public void RemoveUserFromTent(uint tentId, RoomUser user)
    {
        if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
        {
            if (!_tents.ContainsKey(tentId))
                _tents.Add(tentId, new());
            if (_tents[tentId].Contains(user))
                _tents[tentId].Remove(user);
            user.GetClient().GetHabbo().TentId = 0;
        }
    }

    public void SendToTent(int id, uint tentId, IServerPacket packet)
    {
        if (!_tents.ContainsKey(tentId))
            return;
        foreach (var user in _tents[tentId].ToList())
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().IgnoresComponent.IsIgnored(id) ||
                user.GetClient().GetHabbo().TentId != tentId)
                continue;
            user.GetClient().Send(packet);
        }
    }

    public void SendPacket(IServerPacket packet, bool withRightsOnly = false)
    {
        if (packet == null)
            return;
        try
        {
            var users = _roomUserManager.GetUserList().ToList();
            if (_roomUserManager == null || users == null)
                return;
            foreach (var user in users)
            {
                if (user?.GetClient() == null || user.IsBot)
                    continue;
                if (withRightsOnly && !CheckRights(user.GetClient()))
                    continue;
                user.GetClient().Send(packet);
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    public void SendPacket(List<IServerPacket> packets)
    {
        foreach (var packet in packets)
            SendPacket(packet);
    }

    public void Dispose()
    {
        SendPacket(new CloseConnectionComposer());
        if (!MDisposed)
        {
            IsCrashed = false;
            MDisposed = true;
            /* TODO: Needs reviewing */
            try
            {
                if (ProcessTask != null && ProcessTask.IsCompleted)
                    ProcessTask.Dispose();
            }
            catch { }
            TonerData = null;
            MoodlightData = null;
            if (MutedUsers.Count > 0)
                MutedUsers.Clear();
            if (_tents.Count > 0)
                _tents.Clear();
            if (UsersWithRights.Count > 0)
                UsersWithRights.Clear();
            if (_gameManager != null)
            {
                _gameManager.Dispose();
                _gameManager = null;
            }
            if (_freeze != null)
            {
                _freeze.Dispose();
                _freeze = null;
            }
            if (_soccer != null)
            {
                _soccer.Dispose();
                _soccer = null;
            }
            if (_banzai != null)
            {
                _banzai.Dispose();
                _banzai = null;
            }
            if (_gamemap != null)
            {
                _gamemap.Dispose();
                _gamemap = null;
            }
            if (_gameItemHandler != null)
            {
                _gameItemHandler.Dispose();
                _gameItemHandler = null;
            }

            // Room Data?
            if (Teambanzai != null)
            {
                Teambanzai.Dispose();
                Teambanzai = null;
            }
            if (Teamfreeze != null)
            {
                Teamfreeze.Dispose();
                Teamfreeze = null;
            }
            if (_roomUserManager != null)
            {
                _roomUserManager.Dispose();
                _roomUserManager = null;
            }
            if (_roomItemHandling != null)
            {
                _roomItemHandling.Dispose();
                _roomItemHandling = null;
            }
            if (WordFilterList.Count > 0)
                WordFilterList.Clear();
            if (_filterComponent != null)
                _filterComponent.Cleanup();
            if (_wiredComponent != null)
                _wiredComponent.Cleanup();
            if (_bansComponent != null)
                _bansComponent.Cleanup();
            if (_tradingComponent != null)
                _tradingComponent.Cleanup();
        }
    }
}