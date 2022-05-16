using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadReplyComposer : ServerPacket
{
    public ThreadReplyComposer(GameClient session, GroupForumThreadPost post)
            : base(ServerPacketHeader.ThreadReplyMessageComposer)
    {
        Habbo habbo = post.GetAuthor();
        base.WriteInteger(post.ParentThread.ParentForum.Id);
        base.WriteInteger(post.ParentThread.Id);

        base.WriteInteger(post.Id);
        base.WriteInteger(post.ParentThread.Posts.IndexOf(post));

        base.WriteInteger(habbo.Id);
        base.WriteString(habbo.Username);
        base.WriteString(habbo.Look);

        base.WriteInteger((int)(PlusEnvironment.GetUnixTimestamp() - post.Timestamp));
        base.WriteString(post.Message);
        base.WriteByte(0); // User message oculted by - level
        base.WriteInteger(0); // User that oculted message ID
        base.WriteString(""); //Oculted message user name
        base.WriteInteger(10);
        base.WriteInteger(post.ParentThread.GetUserPosts(habbo.Id).Count);
    }
}