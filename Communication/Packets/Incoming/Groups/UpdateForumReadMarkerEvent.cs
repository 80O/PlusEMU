using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateForumReadMarkerEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public UpdateForumReadMarkerEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var length = packet.PopInt();
        for (var i = 0; i < length; i++)
        {
            int forumid = packet.PopInt(); //Forum ID
            int postid = packet.PopInt(); //Post ID
            bool readall = packet.PopBoolean(); //Make all read

            GroupForum forum = _groupForumManager.GetForum(forumid);
            if (forum == null)
                continue;

            var post = forum.GetPost(postid);
            if (post == null)
                continue;

            var thread = post.ParentThread;
            var index = thread.Posts.IndexOf(post);
            thread.AddView(session.GetHabbo().Id, index + 1);
    
        }

        return Task.CompletedTask;
    }
}
