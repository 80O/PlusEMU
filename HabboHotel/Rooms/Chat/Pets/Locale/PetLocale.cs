using System.Data;

namespace Plus.HabboHotel.Rooms.Chat.Pets.Locale;

public class PetLocale : IPetLocale
{
    private Dictionary<string, string[]> _values;

    public PetLocale()
    {
        _values = new Dictionary<string, string[]>();
    }

    public void Init()
    {
        _values = new Dictionary<string, string[]>();
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `bots_pet_responses`");
        var pets = dbClient.GetTable();
        if (pets != null)
            foreach (DataRow row in pets.Rows)
                _values.Add(row[0].ToString(), row[1].ToString().Split(';'));
    }

    public string[] GetValue(string key)
    {
        string[] value;
        if (_values.TryGetValue(key, out value))
            return value;
        return new[] { "Unknown pet speach:" + key };
    }
}