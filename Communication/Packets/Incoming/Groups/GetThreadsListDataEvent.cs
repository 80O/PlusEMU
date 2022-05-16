using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetThreadsListDataEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetThreadsListDataEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient session, ClientPacket packet)
    {
        int forumId = packet.PopInt();
        int startIndex = packet.PopInt();
        int threadCountLength = packet.PopInt();

        GroupForum Forum = _groupForumManager.GetForum(forumId);
        if (Forum == null)
        {
            return Task.CompletedTask;
        }

        session.SendPacket(new ThreadsListDataComposer(Forum, session, startIndex, threadCountLength));
        return Task.CompletedTask;
    }
}