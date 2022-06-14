using Plus.HabboHotel.Rooms.Trading;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Rooms.Instance;

public class TradingComponent
{
    private readonly ConcurrentDictionary<int, Trade> _activeTrades;
    private readonly Room _instance;
    private int _currentId;

    public TradingComponent(Room instance)
    {
        _currentId = 1;
        _instance = instance;
        _activeTrades = new ConcurrentDictionary<int, Trade>();
    }

    public bool StartTrade(RoomUser player1, RoomUser player2, out Trade trade)
    {
        _currentId++;
        trade = new Trade(_currentId, player1, player2, _instance);
        return _activeTrades.TryAdd(_currentId, trade);
    }

    public bool TryGetTrade(int tradeId, out Trade trade) => _activeTrades.TryGetValue(tradeId, out trade);

    public bool RemoveTrade(int id)
    {
        Trade trade = null;
        return _activeTrades.TryRemove(id, out trade);
    }

    public void Cleanup()
    {
        foreach (var trade in _activeTrades.Values)
        {
            foreach (var user in trade.Users)
            {
                if (user == null || user.RoomUser == null)
                    continue;
                trade.EndTrade(user.RoomUser.HabboId);
            }
        }
    }
}