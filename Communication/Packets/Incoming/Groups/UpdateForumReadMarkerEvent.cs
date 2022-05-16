using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateForumReadMarkerEvent : IPacketEvent
{
    public Task Parse(GameClient Session, ClientPacket Packet)
    {
        var length = Packet.PopInt();
        for (var i = 0; i < length; i++)
        {
            int forumid = Packet.PopInt(); //Forum ID
            int postid = Packet.PopInt(); //Post ID
            bool readall = Packet.PopBoolean(); //Make all read

            GroupForum forum = PlusEnvironment.GetGame().GetGroupForumManager().GetForum(forumid);
            if (forum == null)
                continue;

            var post = forum.GetPost(postid);
            if (post == null)
                continue;

            var thread = post.ParentThread;
            var index = thread.Posts.IndexOf(post);
            thread.AddView(Session.GetHabbo().Id, index + 1);
    
        }

        return Task.CompletedTask;
    }
}
