using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;
internal class ThreadUpdatedComposer : ServerPacket
{
    public ThreadUpdatedComposer(GameClient session, GroupForumThread thread)
            : base(ServerPacketHeader.ThreadUpdatedMessageComposer)
    {
        base.WriteInteger(thread.ParentForum.Id);
        thread.SerializeData(session, this);
    }
}
