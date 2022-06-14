using Plus.Database;
using System.Data;

namespace Plus.HabboHotel.Catalog.Pets;

public class PetRaceManager : IPetRaceManager
{
    private readonly IDatabase _database;
    private readonly List<PetRace> _races = new();

    public PetRaceManager(IDatabase database)
    {
        _database = database;
    }
    public void Init()
    {
        if (_races.Count > 0)
            _races.Clear();
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("SELECT * FROM `catalog_pet_races`");
        var data = dbClient.GetTable();
        if (data != null)
        {
            foreach (DataRow row in data.Rows)
            {
                var race = new PetRace(Convert.ToInt32(row["raceid"]), Convert.ToInt32(row["color1"]), Convert.ToInt32(row["color2"]), Convert.ToString(row["has1color"]) == "1",
                    Convert.ToString(row["has2color"]) == "1");
                if (!_races.Contains(race))
                    _races.Add(race);
            }
        }
    }

    public List<PetRace> GetRacesForRaceId(int raceId)
    {
        return _races.Where(race => race.RaceId == raceId).ToList();
    }
}