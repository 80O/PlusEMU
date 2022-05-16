using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadCreatedComposer : ServerPacket
{

    public ThreadCreatedComposer(GameClient session, GroupForumThread thread)
            : base(ServerPacketHeader.ThreadCreatedMessageComposer)
    {
        base.WriteInteger(thread.ParentForum.Id);
        thread.SerializeData(session, this);
    }
}
