using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class DeleteGroupPostEvent : IPacketEvent
{
    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        int forumId = Packet.PopInt();
        int threadId = Packet.PopInt();
        int postId = Packet.PopInt();
        int deleteLevel = Packet.PopInt();

        GroupForum Forum = PlusEnvironment.GetGame().GetGroupForumManager().GetForum(forumId);
        GroupForumThread Thread = Forum.GetThread(threadId);
        GroupForumThreadPost Post = Thread.GetPost(postId);

        Post.DeletedLevel = deleteLevel / 10;
        Post.DeleterId = Session.GetHabbo().Id;

        await Post.Save();

        Session.SendPacket(new PostUpdatedComposer(Session, Post));

        if (Post.DeletedLevel != 0)
            Session.SendWhisper("Success! Forum message has been hidden from view");
        else
            Session.SendWhisper("Success! Forum message has been restored to public view");
    }
}
