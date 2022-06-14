using Plus.Utilities;
using System.Data;
using System.Text.RegularExpressions;

namespace Plus.HabboHotel.Rooms.Chat.Filter;

public sealed class WordFilterManager : IWordFilterManager
{
    private readonly List<WordFilter> _filteredWords;

    public WordFilterManager()
    {
        _filteredWords = new List<WordFilter>();
    }

    public void Init()
    {
        if (_filteredWords.Count > 0)
            _filteredWords.Clear();
        DataTable data = null;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `wordfilter`");
        data = dbClient.GetTable();
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                var isStrict = ConvertExtensions.EnumToBool(row["strict"].ToString());
                var isBannable = ConvertExtensions.EnumToBool(row["bannable"].ToString());
                _filteredWords.Add(new WordFilter(
                    row["word"].ToString(),
                    row["replacement"].ToString(),
                    isStrict,
                    isBannable)
                );
            }
        }
    }

    public string CheckMessage(string message)
    {
        foreach (var filter in _filteredWords.ToList())
        {
            if (message.ToLower().Contains(filter.Word) && filter.IsStrict || message == filter.Word)
                message = Regex.Replace(message, filter.Word, filter.Replacement, RegexOptions.IgnoreCase);
            else if (message.ToLower().Contains(filter.Word) && !filter.IsStrict || message == filter.Word)
            {
                var words = message.Split(' ');
                message = "";
                foreach (var word in words.ToList())
                {
                    if (word.ToLower() == filter.Word)
                        message += filter.Replacement + " ";
                    else
                        message += word + " ";
                }
            }
        }
        return message.TrimEnd(' ');
    }

    public bool CheckBannedWords(string message)
    {
        message = message.Replace(" ", "").Replace(".", "").Replace("_", "").ToLower();
        foreach (var filter in _filteredWords.ToList())
        {
            if (!filter.IsBannable)
                continue;
            if (message.Contains(filter.Word))
                return true;
        }
        return false;
    }

    public bool IsFiltered(string message)
    {
        foreach (var filter in _filteredWords.ToList())
        {
            if (message.Contains(filter.Word))
                return true;
        }
        return false;
    }
}