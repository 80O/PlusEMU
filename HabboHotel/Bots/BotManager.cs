using NLog;
using Plus.Database;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Responses;
using System.Data;

namespace Plus.HabboHotel.Bots;

public class BotManager : IBotManager
{
    private readonly IDatabase _database;
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Bots.BotManager");

    private readonly List<BotResponse> _responses;

    public BotManager(IDatabase database)
    {
        _responses = new List<BotResponse>();
        _database = database;
    }

    public void Init()
    {
        if (_responses.Count > 0)
            _responses.Clear();
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("SELECT `bot_ai`,`chat_keywords`,`response_text`,`response_mode`,`response_beverage` FROM `bots_responses`");
        var data = dbClient.GetTable();
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                _responses.Add(new BotResponse(Convert.ToString(row["bot_ai"]), Convert.ToString(row["chat_keywords"]), Convert.ToString(row["response_text"]), row["response_mode"].ToString(),
                    Convert.ToString(row["response_beverage"])));
            }
        }
    }

    public BotResponse GetResponse(BotAiType type, string message)
    {
        foreach (var response in _responses.Where(x => x.AiType == type).ToList())
            if (response.KeywordMatched(message))
                return response;
        return null;
    }
}