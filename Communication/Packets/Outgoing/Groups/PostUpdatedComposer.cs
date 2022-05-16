using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class PostUpdatedComposer : ServerPacket
{
    public PostUpdatedComposer(GameClient Session, GroupForumThreadPost Post)
            : base(ServerPacketHeader.PostUpdatedMessageComposer)
    {
        base.WriteInteger(Post.ParentThread.ParentForum.Id);
        base.WriteInteger(Post.ParentThread.Id);

        Post.SerializeData(this);
    }
}
