using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetForumStatsEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetForumStatsEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient session, ClientPacket packet)
    {
        int groupForumId = packet.PopInt();

        if (!_groupForumManager.TryGetForum(groupForumId, out GroupForum Forum))
        {
            session.SendWhisper("Oops! This group forum does not exist!");
            return Task.CompletedTask;
        }

        session.SendPacket(new ForumDataComposer(Forum, session));
        return Task.CompletedTask;
    }
}
