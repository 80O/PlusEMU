using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadUpdatedComposer : ServerPacket
{
    public ThreadUpdatedComposer(GameClient Session, GroupForumThread Thread)
            : base(ServerPacketHeader.ThreadUpdatedMessageComposer)
    {
        base.WriteInteger(Thread.ParentForum.Id);
        Thread.SerializeData(Session, this);
    }
}
