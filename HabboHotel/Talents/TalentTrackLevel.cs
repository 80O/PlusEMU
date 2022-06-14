using System.Data;

namespace Plus.HabboHotel.Talents;

public class TalentTrackLevel
{
    private readonly Dictionary<int, TalentTrackSubLevel> _subLevels;

    public TalentTrackLevel(string type, int level, string dataActions, string dataGifts)
    {
        Type = type;
        Level = level;
        foreach (var str in dataActions.Split('|'))
        {
            if (Actions == null) Actions = new List<string>();
            Actions.Add(str);
        }
        foreach (var str in dataGifts.Split('|'))
        {
            if (Gifts == null) Gifts = new List<string>();
            Gifts.Add(str);
        }
        _subLevels = new Dictionary<int, TalentTrackSubLevel>();
        Init();
    }

    public string Type { get; set; }
    public int Level { get; set; }

    public List<string> Actions { get; }

    public List<string> Gifts { get; }

    public void Init()
    {
        DataTable getTable = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `sub_level`,`badge_code`,`required_progress` FROM `talents_sub_levels` WHERE `talent_level` = @TalentLevel");
            dbClient.AddParameter("TalentLevel", Level);
            getTable = dbClient.GetTable();
        }
        if (getTable != null)
        {
            foreach (DataRow row in getTable.Rows)
            {
                _subLevels.Add(Convert.ToInt32(row["sub_level"]),
                    new TalentTrackSubLevel(Convert.ToInt32(row["sub_level"]), Convert.ToString(row["badge_code"]), Convert.ToInt32(row["required_progress"])));
            }
        }
    }

    public ICollection<TalentTrackSubLevel> GetSubLevels() => _subLevels.Values;
}