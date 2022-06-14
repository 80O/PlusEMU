using NLog;
using Plus.Database;
using System.Data;

namespace Plus.HabboHotel.Games;

public class GameDataManager : IGameDataManager
{
    private readonly IDatabase _database;
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Games.GameDataManager");

    private readonly Dictionary<int, GameData> _games;

    public GameDataManager(IDatabase database)
    {
        _games = new Dictionary<int, GameData>();
        _database = database;
    }

    public ICollection<GameData> GameData => _games.Values;

    public void Init()
    {
        if (_games.Count > 0)
            _games.Clear();
        using (var dbClient = _database.GetQueryReactor())
        {
            DataTable data = null;
            dbClient.SetQuery(
                "SELECT `id`,`name`,`colour_one`,`colour_two`,`resource_path`,`string_three`,`game_swf`,`game_assets`,`game_server_host`,`game_server_port`,`socket_policy_port`,`game_enabled` FROM `games_config`");
            data = dbClient.GetTable();
            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    _games.Add(Convert.ToInt32(row["id"]),
                        new GameData(Convert.ToInt32(row["id"]), Convert.ToString(row["name"]), Convert.ToString(row["colour_one"]), Convert.ToString(row["colour_two"]),
                            Convert.ToString(row["resource_path"]), Convert.ToString(row["string_three"]), Convert.ToString(row["game_swf"]), Convert.ToString(row["game_assets"]),
                            Convert.ToString(row["game_server_host"]), Convert.ToString(row["game_server_port"]), Convert.ToString(row["socket_policy_port"]),
                           PlusEnvironment.EnumToBool(row["game_enabled"].ToString())));
                }
            }
        }
        Log.Info("Game Data Manager -> LOADED");
    }

    public bool TryGetGame(int gameId, out GameData data) => _games.TryGetValue(gameId, out data);

    public int GetCount()
    {
        return _games.Values.Count(x => x.Enabled);
    }
}