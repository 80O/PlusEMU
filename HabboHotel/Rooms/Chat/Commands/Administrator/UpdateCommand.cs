using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Users.Clothing;
using Plus.Core.Settings;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Televisions;
using Plus.HabboHotel.LandingView;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Rooms.Chat.Pets.Locale;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator;

internal class UpdateCommand : IChatCommand
{
    private readonly ICatalogManager _catalogManager;
    private readonly IGameClientManager _clientManager;
    private readonly IItemDataManager _itemDataManager;
    private readonly IRoomManager _roomManager;
    private readonly ILandingViewManager _landingViewManager;
    private readonly ITelevisionManager _televisionManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IModerationManager _moderationManager;
    private readonly INavigatorManager _navigatorManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IQuestManager _questManager;
    private readonly IWordFilterManager _wordFilterManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IGameDataManager _gameDataManager;
    private readonly IFigureDataManager _figureDataManager;
    private readonly IBotManager _botManager;
    private readonly IBadgeManager _badgeManager;
    private readonly IRewardManager _rewardManager;
    private readonly IPetLocale _petLocale;
    private readonly IChatStyleManager _chatStyleManager;
    public string Key => "update";
    public string PermissionRequired => "command_update";

    public string Parameters => "%variable%";

    public string Description => "Reload a specific part of the hotel.";

    public UpdateCommand(ICatalogManager catalogManager,
        IGameClientManager clientManager,
        IItemDataManager itemDataManager,
        IRoomManager roomManager,
        ILandingViewManager landingViewManager,
        ITelevisionManager televisionManager,
        ISettingsManager settingsManager,
        IModerationManager moderationManager,
        INavigatorManager navigatorManager,
        IPermissionManager permissionManager,
        IQuestManager questManager,
        IWordFilterManager wordFilterManager,
        IAchievementManager achievementManager,
        IGameDataManager gameDataManager,
        IFigureDataManager figureDataManager,
        IBotManager botManager,
        IBadgeManager badgeManager,
        IRewardManager rewardManager,
        IPetLocale petLocale,
        IChatStyleManager chatStyleManager)
    {
        _catalogManager = catalogManager;
        _clientManager = clientManager;
        _itemDataManager = itemDataManager;
        _roomManager = roomManager;
        _landingViewManager = landingViewManager;
        _televisionManager = televisionManager;
        _settingsManager = settingsManager;
        _moderationManager = moderationManager;
        _navigatorManager = navigatorManager;
        _permissionManager = permissionManager;
        _questManager = questManager;
        _wordFilterManager = wordFilterManager;
        _achievementManager = achievementManager;
        _gameDataManager = gameDataManager;
        _figureDataManager = figureDataManager;
        _botManager = botManager;
        _badgeManager = badgeManager;
        _rewardManager = rewardManager;
        _petLocale = petLocale;
        _chatStyleManager = chatStyleManager;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (parameters.Length == 0)
        {
            session.SendWhisper("You must inculde a thing to update, e.g. :update catalog");
            return;
        }
        var updateVariable = parameters[0];
        switch (updateVariable.ToLower())
        {
            case "cata":
            case "catalog":
            case "catalogue":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_catalog"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.");
                    break;
                }
                _catalogManager.Init();
                _clientManager.SendPacket(new CatalogUpdatedComposer());
                session.SendWhisper("Catalogue successfully updated.");
                break;
            }
            case "items":
            case "furni":
            case "furniture":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_furni"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_furni' permission.");
                    break;
                }
                _itemDataManager.Init();
                session.SendWhisper("Items successfully updated.");
                break;
            }
            case "models":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_models"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_models' permission.");
                    break;
                }
                _roomManager.LoadModels();
                session.SendWhisper("Room models successfully updated.");
                break;
            }
            case "promotions":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_promotions"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_promotions' permission.");
                    break;
                }
                _landingViewManager.Reload();
                session.SendWhisper("Landing view promotions successfully updated.");
                break;
            }
            case "youtube":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_youtube"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_youtube' permission.");
                    break;
                }
                _televisionManager.Init();
                session.SendWhisper("Youtube televisions playlist successfully updated.");
                break;
            }
            case "filter":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_filter"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_filter' permission.");
                    break;
                }
                _wordFilterManager.Init();
                session.SendWhisper("Filter definitions successfully updated.");
                break;
            }
            case "navigator":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_navigator"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_navigator' permission.");
                    break;
                }
                _navigatorManager.Init();
                session.SendWhisper("Navigator items successfully updated.");
                break;
            }
            case "ranks":
            case "rights":
            case "permissions":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_rights"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_rights' permission.");
                    break;
                }
                _permissionManager.Init();
                foreach (var client in _clientManager.GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetHabbo().Permissions == null)
                        continue;
                    client.GetHabbo().Permissions.Init(client.GetHabbo());
                }
                session.SendWhisper("Rank definitions successfully updated.");
                break;
            }
            case "config":
            case "settings":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_configuration"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_configuration' permission.");
                    break;
                }
                _settingsManager.Reload();
                session.SendWhisper("Server configuration successfully updated.");
                break;
            }
            case "bans":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_bans"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_bans' permission.");
                    break;
                }
                _moderationManager.ReCacheBans();
                session.SendWhisper("Ban cache re-loaded.");
                break;
            }
            case "quests":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_quests"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_quests' permission.");
                    break;
                }
                _questManager.Init();
                session.SendWhisper("Quest definitions successfully updated.");
                break;
            }
            case "achievements":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_achievements"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_achievements' permission.");
                    break;
                }
                _achievementManager.Init();
                session.SendWhisper("Achievement definitions bans successfully updated.");
                break;
            }
            case "moderation":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_moderation"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_moderation' permission.");
                    break;
                }
                _moderationManager.Init();
                _clientManager.ModAlert("Moderation presets have been updated. Please reload the client to view the new presets.");
                session.SendWhisper("Moderation configuration successfully updated.");
                break;
            }
            case "vouchers":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_vouchers"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_vouchers' permission.");
                    break;
                }
                _catalogManager.VoucherManager.Init();
                session.SendWhisper("Catalogue vouche cache successfully updated.");
                break;
            }
            case "gc":
            case "games":
            case "gamecenter":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_game_center"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_game_center' permission.");
                    break;
                }
                _gameDataManager.Init();
                session.SendWhisper("Game Center cache successfully updated.");
                break;
            }
            case "pet_locale":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_pet_locale"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_pet_locale' permission.");
                    break;
                }
                _petLocale.Init();
                session.SendWhisper("Pet locale cache successfully updated.");
                break;
            }
            case "locale":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_locale"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_locale' permission.");
                    break;
                }
                PlusEnvironment.LanguageManager.Reload();
                session.SendWhisper("Locale cache successfully updated.");
                break;
            }
            case "mutant":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_anti_mutant"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_anti_mutant' permission.");
                    break;
                }
                    _figureDataManager.UpdateFigureData().Wait();
                    session.SendWhisper("FigureData manager successfully reloaded.");
                break;
            }
            case "bots":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_bots"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_bots' permission.");
                    break;
                }
                _botManager.Init();
                session.SendWhisper("Bot managaer successfully reloaded.");
                break;
            }
            case "rewards":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_rewards"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_rewards' permission.");
                    break;
                }
                _rewardManager.Init();
                session.SendWhisper("Rewards managaer successfully reloaded.");
                break;
            }
            case "chat_styles":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_chat_styles"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_chat_styles' permission.");
                    break;
                }
                _chatStyleManager.Init();
                session.SendWhisper("Chat Styles successfully reloaded.");
                break;
            }
            case "badge_definitions":
            {
                if (!session.GetHabbo().Permissions.HasCommand("command_update_badge_definitions"))
                {
                    session.SendWhisper("Oops, you do not have the 'command_update_badge_definitions' permission.");
                    break;
                }
                _badgeManager.Init();
                session.SendWhisper("Badge definitions successfully reloaded.");
                break;
            }
            default:
                session.SendWhisper($"'{updateVariable}' is not a valid thing to reload.");
                break;
        }
    }
}