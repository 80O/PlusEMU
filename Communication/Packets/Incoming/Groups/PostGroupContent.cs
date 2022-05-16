using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class PostGroupContentEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public PostGroupContentEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        int ForumId = Packet.PopInt();
        int ThreadId = Packet.PopInt();
        string Caption = Packet.PopString();
        string Message = Packet.PopString();

        GroupForum Forum = _groupForumManager.GetForum(ForumId);
        if (Forum == null)
        {
            return;
        }

        bool IsNewThread = ThreadId == 0;
        
        if (IsNewThread)
        {

            if ((Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanInitDiscussions)) != "")
            {
                Session.SendWhisper("Oops! Something went wrong. You do not have permission to start discussions here.");
            }

            GroupForumThread? Thread = await Forum.CreateThread(Session.GetHabbo().Id, Caption);
            GroupForumThreadPost? Post = await Thread.CreatePost(Session.GetHabbo().Id, Message);
            Session.SendPacket(new ThreadCreatedComposer(Session, Thread));

            Thread.AddView(Session.GetHabbo().Id, 1);

        }
        else
        {
            GroupForumThread Thread = Forum.GetThread(ThreadId);
            if (Thread == null)
            {
                return;
            }

            if (Thread.Locked && Forum?.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate) != "")
            {
                Session.SendWhisper("Oops! You can't comment on this forum.");
            }

            if ((Forum?.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanPost)) != "")
            {
                Session.SendWhisper("Oops! You cannot post here.");
            }

            GroupForumThreadPost? Post = await Thread.CreatePost(Session.GetHabbo().Id, Message);
            Session.SendPacket(new ThreadReplyComposer(Session, Post));
        }


    }
}
