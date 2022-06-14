using Plus.Communication.Attributes;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Authentication;
using Plus.HabboHotel.Users.Messenger.FriendBar;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class SsoTicketEvent : IPacketEvent
{
    private readonly IAuthenticator _authenticate;
    private readonly IBadgeManager _badgeManager;

    public SsoTicketEvent(IAuthenticator authenticate, IBadgeManager badgeManager)
    {
        _authenticate = authenticate;
        _badgeManager = badgeManager;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var sso = packet.PopString();
        var error = await _authenticate.AuthenticateUsingSSO(session, sso);
        if (error == null)
        {
            session.SendPacket(new AuthenticationOkComposer());

            // TODO: 80O: Move to individual incoming message handlers.
            session.SendPacket(new AvatarEffectsComposer(session.GetHabbo().Effects().GetAllEffects));
            session.SendPacket(new NavigatorSettingsComposer(session.GetHabbo().HomeRoom));
            session.SendPacket(new FavouritesComposer(session.GetHabbo().FavoriteRooms));
            session.SendPacket(new FigureSetIdsComposer(session.GetHabbo().GetClothing().GetClothingParts));
            session.SendPacket(new UserRightsComposer(session.GetHabbo().Rank, session.GetHabbo().IsAmbassador));
            session.SendPacket(new AvailabilityStatusComposer());
            session.SendPacket(new AchievementScoreComposer(session.GetHabbo().GetStats().AchievementPoints));
            session.SendPacket(new BuildersClubMembershipComposer());
            session.SendPacket(new CfhTopicsInitComposer(PlusEnvironment.GetGame().GetModerationManager().UserActionPresets));
            session.SendPacket(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager().Achievements));
            session.SendPacket(new SoundSettingsComposer(session.GetHabbo().ClientVolume, session.GetHabbo().ChatPreference, session.GetHabbo().AllowMessengerInvites,
                session.GetHabbo().FocusPreference,
                FriendBarStateUtility.GetInt(session.GetHabbo().FriendbarState)));
            //SendMessage(new TalentTrackLevelComposer());


            if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(session.GetHabbo().Rank, out var group))
            {
                if (!string.IsNullOrEmpty(group.Badge))
                {
                    if (!session.GetHabbo().Inventory.Badges.HasBadge(group.Badge))
                        await _badgeManager.GiveBadge(session.GetHabbo(), group.Badge);
                }
            }
            if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(session.GetHabbo().VipRank, out var subData))
            {
                if (!string.IsNullOrEmpty(subData.Badge))
                {
                    if (!session.GetHabbo().Inventory.Badges.HasBadge(subData.Badge))
                        await _badgeManager.GiveBadge(session.GetHabbo(), subData.Badge);
                }
            }
            if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(session.GetHabbo().Id))
                PlusEnvironment.GetGame().GetCacheManager().GenerateUser(session.GetHabbo().Id);
            session.GetHabbo().Look = PlusEnvironment.GetFigureManager().ProcessFigure(session.GetHabbo().Look, session.GetHabbo().Gender, session.GetHabbo().GetClothing().GetClothingParts, true);
            session.GetHabbo().InitProcess();
            if (session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
            {
                session.SendPacket(new ModeratorInitComposer(
                    PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                    PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                    PlusEnvironment.GetGame().GetModerationManager().GetTickets));
            }
            if (PlusEnvironment.GetSettingsManager().TryGetValue("user.login.message.enabled") == "1")
                session.SendPacket(new MotdNotificationComposer(PlusEnvironment.GetLanguageManager().TryGetValue("user.login.message")));
            await PlusEnvironment.GetGame().GetRewardManager().CheckRewards(session);
        }
    }
}