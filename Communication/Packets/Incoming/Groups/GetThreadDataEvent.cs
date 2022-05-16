using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetThreadDataEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetThreadDataEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient Session, ClientPacket Packet)
    {
        int ForumId = Packet.PopInt();
        int ThreadId = Packet.PopInt();
        int StartIndex = Packet.PopInt();
        int length = Packet.PopInt();

        GroupForum Forum = _groupForumManager.GetForum(ForumId);

        if (Forum == null)
        {
            Session.SendWhisper("Awkward! Forum cannot be found.");
            return Task.CompletedTask;
        }

        GroupForumThread Thread = Forum.GetThread(ThreadId);
        if (Thread == null)
        {
            Session.SendWhisper("Unable to find forum thread.");
            return Task.CompletedTask;
        }

        if (Thread.DeletedLevel > 1 && (Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate) != ""))
        {
            Session.SendWhisper(("You are unable to see this thread as it has been deleted."));
            return Task.CompletedTask;
        }


        Session.SendPacket(new ThreadDataComposer(Thread, StartIndex, length));
        return Task.CompletedTask;
    }
}
