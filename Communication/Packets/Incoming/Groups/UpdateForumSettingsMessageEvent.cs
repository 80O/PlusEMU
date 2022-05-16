using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateForumSettingsMessageEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public UpdateForumSettingsMessageEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        int ForumId = Packet.PopInt();
        int WhoCanRead = Packet.PopInt();
        int WhoCanReply = Packet.PopInt();
        int WhoCanPost = Packet.PopInt();
        int WhoCanMod = Packet.PopInt();

        GroupForum forum = _groupForumManager.GetForum(ForumId);

        if (forum == null)
        {
            Session.SendWhisper("Oops! This forum could not be found.");
        }

        if (forum?.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
        {
            Session.SendWhisper("Oops! The forum could not be changed.");
        }

        forum.Settings.WhoCanRead = WhoCanRead;
        forum.Settings.WhoCanModerate = WhoCanMod;
        forum.Settings.WhoCanPost = WhoCanReply;
        forum.Settings.WhoCanInitDiscussions = WhoCanPost;
        await forum.Settings.Save();

        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModForumCanModerateSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModForumCanPostSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModForumCanPostThrdSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModForumCanReadSeen", 1);

        Session.SendPacket(new ForumDataComposer(forum, Session));
        Session.SendPacket(new ThreadsListDataComposer(forum, Session));
    }
}
