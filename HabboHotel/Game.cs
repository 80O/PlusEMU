using Plus.Communication.Packets;
using Plus.Core;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Televisions;
using Plus.HabboHotel.LandingView;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Talents;

namespace Plus.HabboHotel;

// Game services need to be implemented behind an interface.
// Dependency inject the required services in the IPacketEvent
// This class will be obsolete. Do not reference to Game().<Service> but inject it instead.
public class Game : IGame
{
    private readonly IPacketManager _packetManager;
    private readonly ILandingViewManager _landingViewManager; //TODO: Rename class
    private readonly IGameClientManager _clientManager;
    private readonly IModerationManager _moderationManager;
    private readonly IItemDataManager _itemDataManager;
    private readonly ICatalogManager _catalogManager;
    private readonly ITelevisionManager _televisionManager; //TODO: Initialize from the item manager.
    private readonly INavigatorManager _navigatorManager;
    private readonly IRoomManager _roomManager;
    private readonly IChatManager _chatManager;
    private readonly IGroupManager _groupManager;
    private readonly IQuestManager _questManager;
    private readonly IAchievementManager _achievementManager;

    private IBadgeManager _badgeManager;
    private IBotManager _botManager;
    private ICacheManager _cacheManager;
    private readonly int _cycleSleepTime = 25;
    private IGameDataManager _gameDataManager;
    private IServerStatusUpdater _globalUpdater;
    private IPermissionManager _permissionManager;
    private IRewardManager _rewardManager;
    private ISubscriptionManager _subscriptionManager;
    private ITalentTrackManager _talentTrackManager;
    private bool _cycleActive;

    private bool _cycleEnded;
    private Task _gameCycle;

    public Game(IPacketManager packetManager,
        ILandingViewManager landingViewManager,
        IGameClientManager gameClientManager,
        IModerationManager moderationManager,
        IItemDataManager itemDataManager,
        ICatalogManager catalogManager,
        ITelevisionManager televisionManager,
        INavigatorManager navigatorManager,
        IRoomManager roomManager,
        IChatManager chatManager,
        IGroupManager groupManager,
        IQuestManager questManager,
        IAchievementManager achievementManager,
        ITalentTrackManager talentTrackManager,
        IGameDataManager gameDataManager,
        IServerStatusUpdater serverStatusUpdater,
        IBotManager botManager,
        ICacheManager cacheManager,
        IRewardManager rewardManager,
        IBadgeManager badgeManager,
        ISubscriptionManager subscriptionManager,
        IPermissionManager permissionManager)
    {
        _packetManager = packetManager;
        _landingViewManager = landingViewManager;
        _clientManager = gameClientManager;
        _moderationManager = moderationManager;
        _itemDataManager = itemDataManager;
        _catalogManager = catalogManager;
        _televisionManager = televisionManager;
        _navigatorManager = navigatorManager;
        _roomManager = roomManager;
        _chatManager = chatManager;
        _groupManager = groupManager;
        _questManager = questManager;
        _achievementManager = achievementManager;
        _talentTrackManager = talentTrackManager;
        _gameDataManager = gameDataManager;
        _globalUpdater = serverStatusUpdater;
        _botManager = botManager;
        _cacheManager = cacheManager;
        _rewardManager = rewardManager;
        _badgeManager = badgeManager;
        _subscriptionManager = subscriptionManager;
        _permissionManager = permissionManager;
    }

    public Task Init()
    {
        _moderationManager.Init();
        _itemDataManager.Init();
        _catalogManager.Init(_itemDataManager);
        _televisionManager.Init();
        _navigatorManager.Init();
        _roomManager.LoadModels();
        _chatManager.Init();
        _groupManager.Init();
        _questManager.Init();
        _achievementManager.Init();
        _talentTrackManager.Init();
        _gameDataManager.Init();
        _globalUpdater.Init();
        _botManager.Init();
        _rewardManager.Init();
        _badgeManager.Init();
        _permissionManager.Init();
        _subscriptionManager.Init();
        _cacheManager.Init();
        return Task.CompletedTask;
    }

    public void StartGameLoop()
    {
        _gameCycle = new Task(GameCycle);
        _gameCycle.Start();
        _cycleActive = true;
    }

    private void GameCycle()
    {
        while (_cycleActive)
        {
            _cycleEnded = false;
            PlusEnvironment.GetGame().GetRoomManager().OnCycle();
            PlusEnvironment.GetGame().GetClientManager().OnCycle();
            _cycleEnded = true;
            Thread.Sleep(_cycleSleepTime);
        }
    }

    public void StopGameLoop()
    {
        _cycleActive = false;
        while (!_cycleEnded) Thread.Sleep(_cycleSleepTime);
    }

    public IPacketManager GetPacketManager() => _packetManager;

    public IGameClientManager GetClientManager() => _clientManager;

    public ICatalogManager GetCatalog() => _catalogManager;

    public INavigatorManager GetNavigator() => _navigatorManager;

    public IItemDataManager GetItemManager() => _itemDataManager;

    public IRoomManager GetRoomManager() => _roomManager;

    public IAchievementManager GetAchievementManager() => _achievementManager;

    public ITalentTrackManager GetTalentTrackManager() => _talentTrackManager;

    public IModerationManager GetModerationManager() => _moderationManager;

    public IPermissionManager GetPermissionManager() => _permissionManager;

    public ISubscriptionManager GetSubscriptionManager() => _subscriptionManager;

    public IQuestManager GetQuestManager() => _questManager;

    public IGroupManager GetGroupManager() => _groupManager;

    public ILandingViewManager GetLandingManager() => _landingViewManager;

    public ITelevisionManager GetTelevisionManager() => _televisionManager;

    public IChatManager GetChatManager() => _chatManager;

    public IGameDataManager GetGameDataManager() => _gameDataManager;

    public IBotManager GetBotManager() => _botManager;

    public ICacheManager GetCacheManager() => _cacheManager;

    public IRewardManager GetRewardManager() => _rewardManager;

    public IBadgeManager GetBadgeManager() => _badgeManager;
}