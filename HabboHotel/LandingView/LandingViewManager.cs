using Dapper;
using NLog;
using Plus.Core;
using Plus.Database;
using Plus.HabboHotel.LandingView.Promotions;

namespace Plus.HabboHotel.LandingView;

public class LandingViewManager : ILandingViewManager, IStartable
{
    private readonly IDatabase _database;
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.LandingView.LandingViewManager");

    private Dictionary<int, Promotion> _promotionItems = new();

    public LandingViewManager(IDatabase database)
    {
        _database = database;
    }

    public async Task Reload()
    {
        using var connection = _database.Connection();
        _promotionItems = (await connection.QueryAsync<Promotion>("SELECT * FROM `server_landing` ORDER BY `id` DESC")).ToDictionary(promotion => promotion.Id);
        Log.Info("Landing View Manager -> LOADED");
    }

    public ICollection<Promotion> GetPromotionItems() => _promotionItems.Values;
    public Task Start() => Reload();
}