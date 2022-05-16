using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class ModerateThreadMessageEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public ModerateThreadMessageEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var forumId = packet.PopInt();
        var threadId = packet.PopInt();
        var deleteLevel = packet.PopInt();

        GroupForum forum = _groupForumManager.GetForum(forumId);

        if (forum is null)
        {
            return;
        }

        if (forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanModerate) != "")
        {
            session.SendWhisper(("You do not have the required permission to take action on this thread."));
        }

        GroupForumThread thread = forum.GetThread(threadId);

        if (thread is null)
        {
            return;
        }

        thread.DeletedLevel = deleteLevel / 10;
        thread.DeleterUserId = thread.DeletedLevel != 0 ? session.GetHabbo().Id : 0;
        await thread.Save();

        session.SendPacket(new ThreadsListDataComposer(forum, session));

        if (thread.DeletedLevel != 0)
            session.SendWhisper("Successfully removed thread from public view.");
        else
            session.SendWhisper("Thread has been successfully restored to public view.");
    }
}
