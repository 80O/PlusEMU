using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class DeleteGroupPostEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public DeleteGroupPostEvent(IGroupForumManager groupForumManager)
    {
        _groupForumManager = groupForumManager;
    }
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        int forumId = packet.PopInt();
        int threadId = packet.PopInt();
        int postId = packet.PopInt();
        int deleteLevel = packet.PopInt();

        GroupForum forum = _groupForumManager.GetForum(forumId);
        GroupForumThread thread = forum.GetThread(threadId);
        GroupForumThreadPost post = thread.GetPost(postId);

        post.DeletedLevel = deleteLevel / 10;
        post.DeleterId = session.GetHabbo().Id;

        await post.Save();

        session.SendPacket(new PostUpdatedComposer(session, post));

        if (post.DeletedLevel != 0)
            session.SendWhisper("Success! Forum message has been hidden from view");
        else
            session.SendWhisper("Success! Forum message has been restored to public view");
    }
}
