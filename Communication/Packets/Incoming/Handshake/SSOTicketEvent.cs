using Plus.Communication.Attributes;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.HabboHotel.Users.Clothing;
using Plus.Core.Language;
using Plus.Core.Settings;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Users.Authentication;
using Plus.HabboHotel.Users.Messenger.FriendBar;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class SsoTicketEvent : IPacketEvent
{
    private readonly IAuthenticator _authenticate;
    private readonly IBadgeManager _badgeManager;
    private readonly IModerationManager _moderationManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IPermissionManager _permissionManager;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly ICacheManager _cacheManager;
    private readonly IFigureDataManager _figureManager;
    private readonly ILanguageManager _languageManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IRewardManager _rewardManager;

    public SsoTicketEvent(IAuthenticator authenticate,
        IBadgeManager badgeManager,
        IModerationManager moderationManager,
        IAchievementManager achievementManager,
        IPermissionManager permissionManager,
        ISubscriptionManager subscriptionManager,
        ICacheManager cacheManager,
        IFigureDataManager figureManager,
        ILanguageManager languageManager,
        ISettingsManager settingsManager,
        IRewardManager rewardManager)
    {
        _authenticate = authenticate;
        _badgeManager = badgeManager;
        _moderationManager = moderationManager;
        _achievementManager = achievementManager;
        _permissionManager = permissionManager;
        _subscriptionManager = subscriptionManager;
        _cacheManager = cacheManager;
        _figureManager = figureManager;
        _languageManager = languageManager;
        _settingsManager = settingsManager;
        _rewardManager = rewardManager;
    }

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var sso = packet.ReadString();
        var error = await _authenticate.AuthenticateUsingSSO(session, sso);
        if (error == null)
        {
            session.Send(new AuthenticationOkComposer());

            // TODO @80O: Move to individual incoming message handlers.
            session.Send(new AvatarEffectsComposer(session.GetHabbo().Effects.GetAllEffects));
            session.Send(new NavigatorSettingsComposer(session.GetHabbo().HomeRoom));
            session.Send(new FavouritesComposer(session.GetHabbo().FavoriteRooms));
            session.Send(new FigureSetIdsComposer(session.GetHabbo().Clothing.GetClothingParts));
            session.Send(new UserRightsComposer(session.GetHabbo().Rank, session.GetHabbo().IsAmbassador));
            session.Send(new AvailabilityStatusComposer());
            session.Send(new AchievementScoreComposer(session.GetHabbo().HabboStats.AchievementPoints));
            session.Send(new BuildersClubMembershipComposer());
            session.Send(new CfhTopicsInitComposer(_moderationManager.UserActionPresets));
            session.Send(new BadgeDefinitionsComposer(_achievementManager.Achievements));
            session.Send(new SoundSettingsComposer(session.GetHabbo().ClientVolume, session.GetHabbo().ChatPreference, session.GetHabbo().AllowMessengerInvites,
                session.GetHabbo().FocusPreference,
                FriendBarStateUtility.GetInt(session.GetHabbo().FriendbarState)));
            //SendMessage(new TalentTrackLevelComposer());


            if (_permissionManager.TryGetGroup(session.GetHabbo().Rank, out var group))
            {
                if (!string.IsNullOrEmpty(group.Badge))
                {
                    if (!session.GetHabbo().Inventory.Badges.HasBadge(group.Badge))
                        await _badgeManager.GiveBadge(session.GetHabbo(), group.Badge);
                }
            }
            if (_subscriptionManager.TryGetSubscriptionData(session.GetHabbo().VipRank, out var subData))
            {
                if (!string.IsNullOrEmpty(subData.Badge))
                {
                    if (!session.GetHabbo().Inventory.Badges.HasBadge(subData.Badge))
                        await _badgeManager.GiveBadge(session.GetHabbo(), subData.Badge);
                }
            }
            if (!_cacheManager.ContainsUser(session.GetHabbo().Id))
                _cacheManager.GenerateUser(session.GetHabbo().Id);
            session.GetHabbo().Look = _figureManager.ValidateLookAsync(session.GetHabbo().Look, session.GetHabbo().Gender, session.GetHabbo().Clothing.GetClothingParts, true).Result;
            session.GetHabbo().InitProcess();
            if (session.GetHabbo().Permissions.HasRight("mod_tickets"))
            {
                session.Send(new ModeratorInitComposer(
                    _moderationManager.UserMessagePresets,
                    _moderationManager.RoomMessagePresets,
                    _moderationManager.GetTickets));
            }
            if (_settingsManager.TryGetValue("user.login.message.enabled") == "1")
                session.Send(new MotdNotificationComposer(_languageManager.TryGetValue("user.login.message")));
            await _rewardManager.CheckRewards(session);
        }
    }
}