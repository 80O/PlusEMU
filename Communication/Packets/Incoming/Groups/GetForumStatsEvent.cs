using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetForumStatsEvent : IPacketEvent
{
    public Task Parse(GameClient Session, ClientPacket Packet)
    {
        int GroupForumId = Packet.PopInt();

        if (!PlusEnvironment.GetGame().GetGroupForumManager().TryGetForum(GroupForumId, out GroupForum Forum))
        {
            Session.SendWhisper("Oops! This group forum does not exist!");
            return Task.CompletedTask;
        }

        Session.SendPacket(new ForumDataComposer(Forum, Session));
        return Task.CompletedTask;
    }
}
