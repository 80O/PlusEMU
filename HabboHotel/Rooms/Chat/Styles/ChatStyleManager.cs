using System.Data;
using Microsoft.Extensions.Logging;

namespace Plus.HabboHotel.Rooms.Chat.Styles;

public sealed class ChatStyleManager : IChatStyleManager
{
    private readonly ILogger<ChatStyleManager> _logger;

    private readonly Dictionary<int, ChatStyle> _styles;

    public ChatStyleManager(ILogger<ChatStyleManager> logger)
    {
        _logger = logger;
        _styles = new Dictionary<int, ChatStyle>();
    }

    public void Init()
    {
        if (_styles.Count > 0)
            _styles.Clear();
        DataTable table = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `room_chat_styles`;");
            table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        if (!_styles.ContainsKey(Convert.ToInt32(row["id"])))
                            _styles.Add(Convert.ToInt32(row["id"]), new ChatStyle(Convert.ToInt32(row["id"]), Convert.ToString(row["name"]), Convert.ToString(row["required_right"])));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Unable to load ChatBubble for ID [" + Convert.ToInt32(row["id"]) + "]", ex);
                    }
                }
            }
        }
        _logger.LogInformation("Loaded " + _styles.Count + " chat styles.");
    }

    public bool TryGetStyle(int id, out ChatStyle style) => _styles.TryGetValue(id, out style);
}