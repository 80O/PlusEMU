using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetThreadsListDataEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public GetThreadsListDataEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public Task Parse(GameClient Session, ClientPacket Packet)
    {
        int ForumId = Packet.PopInt();
        int StartIndex = Packet.PopInt();
        int ThreadCountLength = Packet.PopInt();

        GroupForum Forum = _groupForumManager.GetForum(ForumId);
        if (Forum == null)
        {
            return Task.CompletedTask;
        }

        Session.SendPacket(new ThreadsListDataComposer(Forum, Session, StartIndex, ThreadCountLength));
        return Task.CompletedTask;
    }
}