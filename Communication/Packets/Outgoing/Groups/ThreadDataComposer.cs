using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadDataComposer : ServerPacket
{
    public ThreadDataComposer(GroupForumThread Thread, int StartIndex, int MaxLength)
            : base(ServerPacketHeader.ThreadDataMessageComposer)
    {
        base.WriteInteger(Thread.ParentForum.Id);
        base.WriteInteger(Thread.Id);
        base.WriteInteger(StartIndex);
        base.WriteInteger(Thread.Posts.Count);

        foreach (var Post in Thread.Posts)
        {
            Post.SerializeData(this);
        }
    }
}
