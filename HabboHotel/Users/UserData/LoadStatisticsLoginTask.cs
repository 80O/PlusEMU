using Plus.Core;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Users.UserData;

internal class LoadStatisticsLoginTask : IUserDataLoadingTask
{
    private readonly IHabboStatsService _habboStatsService;

    public LoadStatisticsLoginTask(IHabboStatsService habboStatsService) => _habboStatsService = habboStatsService ?? throw new ArgumentNullException(nameof(habboStatsService));

    public async Task Load(Habbo habbo)
    {
        if (habbo == null) throw new ArgumentNullException(nameof(habbo));

        try
        {
            var stats = await _habboStatsService.LoadHabboStats(habbo.Id);

            if (stats.RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
            {
                var dailyRespects = 10;
                stats.DailyRespectPoints = dailyRespects;
                stats.DailyPetRespectPoints = dailyRespects;
                stats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");

                await _habboStatsService.UpdateDailyRespectsAndTimestamp(habbo.Id, dailyRespects, stats.RespectsTimestamp);
            }

            if (!PlusEnvironment.Game.GroupManager.TryGetGroup(stats.FavouriteGroupId, out Group g))
            {
                stats.FavouriteGroupId = 0;
            }

            habbo.HabboStats = stats;
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }
}
