using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.LandingView;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Quests;

internal class GetDailyQuestEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;

    public GetDailyQuestEvent(IGameClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var usersOnline = _clientManager.Count;
        session.SendPacket(new ConcurrentUsersGoalProgressComposer(usersOnline));
        return Task.CompletedTask;
    }
}