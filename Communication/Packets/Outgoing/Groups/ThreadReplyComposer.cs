using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadReplyComposer : ServerPacket
{
    public ThreadReplyComposer(GameClient Session, GroupForumThreadPost Post)
            : base(ServerPacketHeader.ThreadReplyMessageComposer)
    {
        var User = Post.GetAuthor();
        base.WriteInteger(Post.ParentThread.ParentForum.Id);
        base.WriteInteger(Post.ParentThread.Id);

        base.WriteInteger(Post.Id);
        base.WriteInteger(Post.ParentThread.Posts.IndexOf(Post));

        base.WriteInteger(User.Id);
        base.WriteString(User.Username);
        base.WriteString(User.Look);

        base.WriteInteger((int)(PlusEnvironment.GetUnixTimestamp() - Post.Timestamp));
        base.WriteString(Post.Message);
        base.WriteByte(0); // User message oculted by - level
        base.WriteInteger(0); // User that oculted message ID
        base.WriteString(""); //Oculted message user name
        base.WriteInteger(10);
        base.WriteInteger(Post.ParentThread.GetUserPosts(User.Id).Count);
    }
}