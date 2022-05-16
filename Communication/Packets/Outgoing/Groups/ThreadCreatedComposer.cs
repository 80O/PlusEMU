using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadCreatedComposer : ServerPacket
{

    public ThreadCreatedComposer(GameClient Session, GroupForumThread Thread)
            : base(ServerPacketHeader.ThreadCreatedMessageComposer)
    {
        base.WriteInteger(Thread.ParentForum.Id);
        Thread.SerializeData(Session, this);
    }
}
