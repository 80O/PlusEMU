using System.Collections;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Users.Clothing;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Users.Ignores;
using Plus.HabboHotel.Users.Inventory;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Users.Permissions;
using Plus.HabboHotel.Users.Process;
using Plus.Utilities;

using Dapper;
using Plus.HabboHotel.Users.Navigator;

namespace Plus.HabboHotel.Users;

public class Habbo
{
    private HabboStats _habboStats;

    //Room related

    private readonly DateTime _timeCached;

    private GameClient _client;
    private ClothingComponent _clothing;

    //Player saving.
    private bool _disconnected;

    //Fastfood

    //Counters
    private EffectsComponent _fx;

    private bool _habboSaved;

    //Advertising reporting system.

    //Generic player values.
    public IgnoresComponent IgnoresComponent { get; set; }
    public InventoryComponent Inventory { get; private set; }

    //Anti-script placeholders.
    private HabboMessenger _messenger;

    private NavigatorPreferences _navigatorPreferences;
    private PermissionComponent _permissions;

    //Just random fun stuff.
    private ProcessComponent _process;

    //Values generated within the game.
    public ConcurrentDictionary<string, UserAchievement> Achievements = new();
    public ArrayList FavoriteRooms = new();
    public Dictionary<int, int> Quests = new();

    public List<int> RatedRooms = new();

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public int Rank { get; set; }

    public bool IsAmbassador { get; set; }

    public string Motto { get; set; } = string.Empty;

    public string Look { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string FootballLook { get; set; } = string.Empty;

    public string FootballGender { get; set; } = string.Empty;

    public int Credits { get; set; }

    public int Duckets { get; set; }

    public int Diamonds { get; set; }

    public int GotwPoints { get; set; }

    public int HomeRoom { get; set; }

    public double LastOnline { get; set; }

    public double AccountCreated { get; set; }

    public List<int> ClientVolume { get; set; } = new() { 0, 0, 0};

    public double LastNameChange { get; set; }

    public string MachineId { get; set; }

    public bool ChatPreference { get; set; }

    public bool FocusPreference { get; set; }

    public bool IsExpert { get; set; }

    public int VipRank { get; set; }

    public int TempInt { get; set; }

    public bool AllowTradingRequests { get; set; }

    public bool AllowUserFollowing { get; set; }

    public bool AllowFriendRequests { get; set; }

    public bool AllowMessengerInvites { get; set; }

    public bool AllowPetSpeech { get; set; }

    public bool AllowBotSpeech { get; set; }

    public bool AllowPublicRoomStatus { get; set; }

    public bool AllowConsoleMessages { get; set; } = true;

    public bool AllowGifts { get; set; }

    public bool AllowMimic { get; set; }

    public bool ReceiveWhispers { get; set; }

    public bool IgnorePublicWhispers { get; set; }

    public bool PlayingFastFood { get; set; }

    public FriendBarState FriendbarState { get; set; }

    public int ChristmasDay { get; set; }

    public int WantsToRideHorse { get; set; }

    public int TimeAfk { get; set; }

    public bool DisableForcedEffects { get; set; }

    public bool ChangingName { get; set; }

    public int FriendCount { get; set; }

    public double FloodTime { get; set; }

    public int BannedPhraseCount { get; set; }

    public bool RoomAuthOk { get; set; }

    public int CurrentRoomId { get; set; }

    public int QuestLastCompleted { get; set; }

    public int MessengerSpamCount { get; set; }

    public double MessengerSpamTime { get; set; }

    public double TimeMuted { get; set; }

    public double TradingLockExpiry { get; set; }

    public double SessionStart { get; set; }

    public int TentId { get; set; }

    public int HopperId { get; set; }

    public bool IsHopping { get; set; }

    public int TeleporterId { get; set; }

    public bool IsTeleporting { get; set; }

    public int TeleportingRoomId { get; set; }

    public bool HasSpoken { get; set; }

    public double LastAdvertiseReport { get; set; }

    public bool AdvertisingReported { get; set; }

    public bool AdvertisingReportedBlocked { get; set; }

    public bool WiredInteraction { get; set; }

    public bool InventoryAlert { get; set; }

    public bool IgnoreBobbaFilter { get; set; }

    public bool WiredTeleporting { get; set; }

    public int CustomBubbleId { get; set; }

    public bool OnHelperDuty { get; set; }

    public int FastfoodScore { get; set; }

    public int PetId { get; set; }

    public int CreditsUpdateTick { get; set; }

    public ICommandBase ChatCommand { get; set; }

    public DateTime LastGiftPurchaseTime { get; set; }

    public DateTime LastMottoUpdateTime { get; set; }

    public DateTime LastClothingUpdateTime { get; set; }

    public DateTime LastForumMessageUpdateTime { get; set; }

    public int GiftPurchasingWarnings { get; set; }

    public int MottoUpdateWarnings { get; set; }

    public int ClothingUpdateWarnings { get; set; }

    public bool SessionGiftBlocked { get; set; }

    public bool SessionMottoBlocked { get; set; }

    public bool SessionClothingBlocked { get; set; }

    public bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

    public Room? CurrentRoom
    {
        // TODO: Cache Room instead of fetching it from RoomManager
        get
        {
            if (CurrentRoomId <= 0)
                return null;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(CurrentRoomId, out var room))
                return room;
            return null;
        }
    }

    public string GetQueryString
    {
        get
        {
            _habboSaved = true;
            return "UPDATE `users` SET `online` = '0', `last_online` = '" + UnixTimestamp.GetNow() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits +
                   "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GotwPoints + "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" +
                   FriendBarStateUtility.GetInt(FriendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_statistics` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" +
                   (UnixTimestamp.GetNow() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven +
                   "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints +
                   "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId +
                   "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "' WHERE `id` = '" +
                   Id + "' LIMIT 1;";
        }
    }

    public HabboStats GetStats() => _habboStats;

    public bool CacheExpired()
    {
        var span = DateTime.Now - _timeCached;
        return span.TotalMinutes >= 30;
    }

    public bool InitProcess()
    {
        _process = new ProcessComponent();
        return _process.Init(this);
    }

    public bool InitFx()
    {
        _fx = new EffectsComponent();
        return _fx.Init(this);
    }

    public bool InitClothing()
    {
        _clothing = new ClothingComponent();
        return _clothing.Init(this);
    }

    [Obsolete("Each loading task should be moved to their own IUserDataLoadingTask")]
    public void Init(GameClient client)
    {
        // Move each of these loading tasks to their own IUserDataLoadingTask implementation.
        //foreach (var id in data.FavouritedRooms) FavoriteRooms.Add(id);
        _client = client;
        //Quests = data.Quests;
        _disconnected = false;
        InitFx();
        InitClothing();
    }


    public PermissionComponent GetPermissions() => _permissions;

    public event EventHandler? Disconnected;
    public void OnDisconnect()
    {
        if (_disconnected)
            return;

        Disconnected?.Invoke(this, EventArgs.Empty);

        try
        {
            if (_process != null)
                _process.Dispose();
        }
        catch { }
        _disconnected = true;
        PlusEnvironment.GetGame().GetClientManager().UnregisterClient(Id, Username);
        if (!_habboSaved)
        {
            _habboSaved = true;
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.RunQuery("UPDATE `users` SET `online` = '0', `last_online` = '" + (int)UnixTimestamp.GetNow() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits +
                              "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GotwPoints + "', `time_muted` = '" + TimeMuted +
                              "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(FriendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_statistics` SET `roomvisits` = '" +
                              _habboStats.RoomVisits + "', `onlineTime` = '" + (int)(UnixTimestamp.GetNow() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect +
                              "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived +
                              "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" +
                              _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" +
                              _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "', `bubble_id` = '" + CustomBubbleId + "' WHERE `id` = '" + Id + "' LIMIT 1;");
            if (GetPermissions().HasRight("mod_tickets"))
                dbClient.RunQuery("UPDATE `moderation_tickets` SET `status` = 'open', `moderator_id` = '0' WHERE `status` ='picked' AND `moderator_id` = '" + Id + "'");
        }
        Dispose();
        _client = null;
    }

    public void Dispose()
    {
        if (InRoom && CurrentRoom != null)
            CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(_client, false);
        if (_fx != null)
            _fx.Dispose();
        if (_clothing != null)
            _clothing.Dispose();
        if (_permissions != null)
            _permissions.Dispose();
    }

    public void CheckCreditsTimer()
    {
        try
        {
            CreditsUpdateTick--;
            if (CreditsUpdateTick <= 0)
            {
                var creditUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.credit_reward"));
                var ducketUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.ducket_reward"));
                SubscriptionData subData = null;
                if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VipRank, out subData))
                {
                    creditUpdate += subData.Credits;
                    ducketUpdate += subData.Duckets;
                }
                Credits += creditUpdate;
                Duckets += ducketUpdate;
                _client.Send(new CreditBalanceComposer(Credits));
                _client.Send(new HabboActivityPointNotificationComposer(Duckets, ducketUpdate));
                CreditsUpdateTick = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.tick"));
            }
        }
        catch { }
    }

    public GameClient GetClient()
    {
        if (_client != null)
            return _client;
        return PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(Id);
    }

    public HabboMessenger GetMessenger() => _messenger;

    public void SetInventoryComponent(InventoryComponent inventory)
    {
        Inventory = inventory;
    }

    public NavigatorPreferences GetNavigatorSearches() => _navigatorPreferences;

    public void SetNavigatorPreferences(NavigatorPreferences navigatorPreferences)
    {
        _navigatorPreferences = navigatorPreferences;
    }

    public EffectsComponent Effects() => _fx;

    public ClothingComponent GetClothing() => _clothing;

    public int GetQuestProgress(int p)
    {
        var progress = 0;
        Quests.TryGetValue(p, out progress);
        return progress;
    }

    public UserAchievement GetAchievementData(string p)
    {
        UserAchievement achievement = null;
        Achievements.TryGetValue(p, out achievement);
        return achievement;
    }

    public void ChangeName(string username)
    {
        LastNameChange = UnixTimestamp.GetNow();
        Username = username;
        SaveKey("username", username);
        SaveKey("last_change", LastNameChange.ToString());
    }

    public void SaveChatBubble(string customBubbleID) => SaveKey("bubble_id", customBubbleID);

    public void SaveKey(string key, string value)
    {
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("UPDATE `users` SET " + key + " = @value WHERE `id` = '" + Id + "' LIMIT 1;");
        dbClient.AddParameter("value", value);
        dbClient.RunQuery();
    }

    public void PrepareRoom(int id, string password)
    {
        if (GetClient() == null || GetClient().GetHabbo() == null)
            return;
        if (GetClient().GetHabbo().InRoom)
        {
            Room oldRoom = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(GetClient().GetHabbo().CurrentRoomId, out oldRoom))
                return;
            if (oldRoom.GetRoomUserManager() != null)
                oldRoom.GetRoomUserManager().RemoveUserFromRoom(GetClient(), false);
        }
        if (GetClient().GetHabbo().IsTeleporting && GetClient().GetHabbo().TeleportingRoomId != id)
        {
            GetClient().Send(new CloseConnectionComposer());
            return;
        }
        Room room = null;
        if (!PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(id, out room))
        {
            GetClient().Send(new CloseConnectionComposer());
            return;
        }
        if (room.IsCrashed)
        {
            GetClient().SendNotification("This room has crashed! :(");
            GetClient().Send(new CloseConnectionComposer());
            return;
        }
        GetClient().GetHabbo().CurrentRoomId = room.RoomId;
        if (room.GetRoomUserManager().UserCount >= room.UsersMax && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") && GetClient().GetHabbo().Id != room.OwnerId)
        {
            GetClient().Send(new CantConnectComposer(1));
            GetClient().Send(new CloseConnectionComposer());
            return;
        }
        if (!GetPermissions().HasRight("room_ban_override") && room.GetBans().IsBanned(Id))
        {
            RoomAuthOk = false;
            GetClient().GetHabbo().RoomAuthOk = false;
            GetClient().Send(new CantConnectComposer(4));
            GetClient().Send(new CloseConnectionComposer());
            return;
        }
        GetClient().Send(new OpenConnectionComposer());
        if (!room.CheckRights(GetClient(), true, true) && !GetClient().GetHabbo().IsTeleporting && !GetClient().GetHabbo().IsHopping)
        {
            if (room.Access == RoomAccess.Doorbell && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
            {
                if (room.UserCount > 0)
                {
                    GetClient().Send(new DoorbellComposer(""));
                    room.SendPacket(new DoorbellComposer(GetClient().GetHabbo().Username), true);
                    return;
                }
                GetClient().Send(new FlatAccessDeniedComposer(""));
                GetClient().Send(new CloseConnectionComposer());
                return;
            }
            if (room.Access == RoomAccess.Password && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
            {
                if (password.ToLower() != room.Password.ToLower() || string.IsNullOrWhiteSpace(password))
                {
                    GetClient().Send(new GenericErrorComposer(-100002));
                    GetClient().Send(new CloseConnectionComposer());
                    return;
                }
            }
        }
        if (!EnterRoom(room))
            GetClient().Send(new CloseConnectionComposer());
    }

    public bool EnterRoom(Room room)
    {
        if (room == null)
            GetClient().Send(new CloseConnectionComposer());
        GetClient().Send(new RoomReadyComposer(room.RoomId, room.ModelName));
        if (room.Wallpaper != "0.0")
            GetClient().Send(new RoomPropertyComposer("wallpaper", room.Wallpaper));
        if (room.Floor != "0.0")
            GetClient().Send(new RoomPropertyComposer("floor", room.Floor));
        GetClient().Send(new RoomPropertyComposer("landscape", room.Landscape));
        GetClient().Send(new RoomRatingComposer(room.Score, !(GetClient().GetHabbo().RatedRooms.Contains(room.RoomId) || room.OwnerId == GetClient().GetHabbo().Id)));


        using (var dbClient = PlusEnvironment.GetDatabaseManager().Connection())
        {
            dbClient.Execute("INSERT INTO user_roomvisits (user_id,room_id,entry_timestamp,exit_timestamp) VALUES (@userId, @roomId, @entryTimestamp, @exitTimestamp)",
                new
                {
                    userId = GetClient().GetHabbo().Id,
                    roomId = GetClient().GetHabbo().CurrentRoomId,
                    entryTimestamp = UnixTimestamp.GetNow(),
                    exitTimestamp = 0,
                });
        }

        if (room.OwnerId != Id)
        {
            GetClient().GetHabbo().GetStats().RoomVisits += 1;
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(GetClient(), "ACH_RoomEntry", 1);
        }
        return true;
    }

    public void SetStats(HabboStats stats)
    {
        _habboStats = stats;
    }

    public void SetPermissions(PermissionComponent permissions)
    {
        _permissions = permissions;
    }

    public void SetMessenger(HabboMessenger messenger)
    {
        _messenger = messenger;
    }
}