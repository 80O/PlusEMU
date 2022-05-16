using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetThreadDataEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetThreadDataEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient session, ClientPacket packet)
    {
        int forumId = packet.PopInt();
        int threadId = packet.PopInt();
        int startIndex = packet.PopInt();
        int length = packet.PopInt();

        GroupForum Forum = _groupForumManager.GetForum(forumId);

        if (Forum == null)
        {
            session.SendWhisper("Awkward! Forum cannot be found.");
            return Task.CompletedTask;
        }

        GroupForumThread Thread = Forum.GetThread(threadId);
        if (Thread == null)
        {
            session.SendWhisper("Unable to find forum thread.");
            return Task.CompletedTask;
        }

        if (Thread.DeletedLevel > 1 && (Forum.Settings.GetReasonForNot(session, Forum.Settings.WhoCanModerate) != ""))
        {
            session.SendWhisper(("You are unable to see this thread as it has been deleted."));
            return Task.CompletedTask;
        }


        session.SendPacket(new ThreadDataComposer(Thread, startIndex, length));
        return Task.CompletedTask;
    }
}
