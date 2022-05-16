using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class PostUpdatedComposer : ServerPacket
{
    public PostUpdatedComposer(GameClient session, GroupForumThreadPost post)
            : base(ServerPacketHeader.PostUpdatedMessageComposer)
    {
        base.WriteInteger(post.ParentThread.ParentForum.Id);
        base.WriteInteger(post.ParentThread.Id);

        post.SerializeData(this);
    }
}
