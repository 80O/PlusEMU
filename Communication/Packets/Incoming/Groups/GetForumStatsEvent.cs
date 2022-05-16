using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetForumStatsEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetForumStatsEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient Session, ClientPacket Packet)
    {
        int GroupForumId = Packet.PopInt();

        if (!_groupForumManager.TryGetForum(GroupForumId, out GroupForum Forum))
        {
            Session.SendWhisper("Oops! This group forum does not exist!");
            return Task.CompletedTask;
        }

        Session.SendPacket(new ForumDataComposer(Forum, Session));
        return Task.CompletedTask;
    }
}
