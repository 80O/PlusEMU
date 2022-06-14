using System.Data;

namespace Plus.HabboHotel.Users.Calendar;

/// <summary>
/// Permissions for a specific Player.
/// </summary>
public sealed class CalendarComponent
{
    /// <summary>
    /// Permission rights are stored here.
    /// </summary>
    private readonly List<int> _lateBoxes;

    private readonly List<int> _openedBoxes;

    public CalendarComponent()
    {
        _lateBoxes = new List<int>();
        _openedBoxes = new List<int>();
    }

    /// <summary>
    /// Initialize the PermissionComponent.
    /// </summary>
    /// <param name="player"></param>
    public bool Init(Habbo player)
    {
        if (_lateBoxes.Count > 0)
            _lateBoxes.Clear();
        if (_openedBoxes.Count > 0)
            _openedBoxes.Clear();
        DataTable getData = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `user_xmas15_calendar` WHERE `user_id` = @id;");
        dbClient.AddParameter("id", player.Id);
        getData = dbClient.GetTable();
        if (getData != null)
        {
            foreach (DataRow row in getData.Rows)
            {
                if (Convert.ToInt32(row["status"]) == 0)
                    _lateBoxes.Add(Convert.ToInt32(row["day"]));
                else
                    _openedBoxes.Add(Convert.ToInt32(row["day"]));
            }
        }
        return true;
    }

    public List<int> GetOpenedBoxes() => _openedBoxes;

    public List<int> GetLateBoxes() => _lateBoxes;

    /// <summary>
    /// Dispose of the permissions list.
    /// </summary>
    public void Dispose()
    {
        _lateBoxes.Clear();
        _openedBoxes.Clear();
    }
}