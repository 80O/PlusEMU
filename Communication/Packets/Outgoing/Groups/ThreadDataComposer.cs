using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadDataComposer : ServerPacket
{
    public ThreadDataComposer(GroupForumThread thread, int startIndex, int maxLength)
            : base(ServerPacketHeader.ThreadDataMessageComposer)
    {
        base.WriteInteger(thread.ParentForum.Id);
        base.WriteInteger(thread.Id);
        base.WriteInteger(startIndex);
        base.WriteInteger(thread.Posts.Count);

        foreach (GroupForumThreadPost Post in thread.Posts)
        {
            Post.SerializeData(this);
        }
    }
}
