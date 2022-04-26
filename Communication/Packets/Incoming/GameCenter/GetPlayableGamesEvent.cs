using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.GameCenter;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.GameCenter;

internal class GetPlayableGamesEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;

    public GetPlayableGamesEvent(IAchievementManager achievementManager)
    {
        _achievementManager = achievementManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var gameId = packet.PopInt();
        session.SendPacket(new GameAccountStatusComposer(gameId));
        session.SendPacket(new PlayableGamesComposer(gameId));
        session.SendPacket(new GameAchievementListComposer(session, _achievementManager.GetGameAchievements(gameId), gameId));
        return Task.CompletedTask;
    }
}