using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateForumSettingsMessageEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public UpdateForumSettingsMessageEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        int forumId = packet.PopInt();
        int whoCanRead = packet.PopInt();
        int whoCanReply = packet.PopInt();
        int whoCanPost = packet.PopInt();
        int whoCanMod = packet.PopInt();

        GroupForum forum = _groupForumManager.GetForum(forumId);

        if (forum == null)
        {
            session.SendWhisper("Oops! This forum could not be found.");
        }

        if (forum?.Settings.GetReasonForNot(session, forum.Settings.WhoCanModerate) != "")
        {
            session.SendWhisper("Oops! The forum could not be changed.");
        }

        forum.Settings.WhoCanRead = whoCanRead;
        forum.Settings.WhoCanModerate = whoCanMod;
        forum.Settings.WhoCanPost = whoCanReply;
        forum.Settings.WhoCanInitDiscussions = whoCanPost;
        await forum.Settings.Save();

        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanModerateSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanPostSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanPostThrdSeen", 1);
        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanReadSeen", 1);

        session.SendPacket(new ForumDataComposer(forum, session));
        session.SendPacket(new ThreadsListDataComposer(forum, session));
    }
}
