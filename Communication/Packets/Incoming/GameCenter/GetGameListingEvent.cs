using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.GameCenter;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Games;

namespace Plus.Communication.Packets.Incoming.GameCenter;

internal class GetGameListingEvent : IPacketEvent
{
    private readonly IGameDataManager _gameDataManager;

    public GetGameListingEvent(IGameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new GameListComposer(_gameDataManager.GameData));
        return Task.CompletedTask;
    }
}