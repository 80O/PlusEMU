using System.Data;
using Microsoft.Extensions.Logging;

namespace Plus.HabboHotel.Talents;

public class TalentTrackManager : ITalentTrackManager
{
    private readonly ILogger<TalentTrackManager> _logger;

    private readonly Dictionary<int, TalentTrackLevel> _citizenshipLevels;

    public TalentTrackManager(ILogger<TalentTrackManager> logger)
    {
        _logger = logger;
        _citizenshipLevels = new Dictionary<int, TalentTrackLevel>();
    }

    public void Init()
    {
        DataTable data = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `type`,`level`,`data_actions`,`data_gifts` FROM `talents`");
            data = dbClient.GetTable();
        }
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                _citizenshipLevels.Add(Convert.ToInt32(row["level"]),
                    new TalentTrackLevel(Convert.ToString(row["type"]), Convert.ToInt32(row["level"]), Convert.ToString(row["data_actions"]), Convert.ToString(row["data_gifts"])));
            }
        }
        _logger.LogInformation("Loaded " + _citizenshipLevels.Count + " talent track levels");
    }

    public ICollection<TalentTrackLevel> GetLevels() => _citizenshipLevels.Values;
}