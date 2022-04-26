using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;

namespace Plus.Communication.Packets.Incoming.GameCenter;

internal class Game2GetWeeklyLeaderboardEvent : IPacketEvent
{
    private readonly IGameDataManager _gameDataManager;

    public Game2GetWeeklyLeaderboardEvent(IGameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;
    }


    public Task Parse(GameClient session, ClientPacket packet)
    {
        var gameId = packet.PopInt();
        if (_gameDataManager.TryGetGame(gameId, out var gameData))
        {
            //Code
        }
        return Task.CompletedTask;
    }
}