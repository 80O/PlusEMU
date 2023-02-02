using Dapper;
using Plus.Database;

namespace Plus.HabboHotel.Catalog.Clothing;

public class ClothingManager : IClothingManager
{
    private readonly IDatabase _database;
    private readonly Dictionary<int, ClothingItem> _clothing;

    public ClothingManager(IDatabase database)
    {
        _database = database;
        _clothing = new();
    }

    public ICollection<ClothingItem> GetClothingAllParts => _clothing.Values;

    public async void Init()
    {
        _clothing.Clear();
        using var connection = _database.Connection();
        var data = await connection.QueryAsync<(int Id, string ClothingName, string PartIds)>("SELECT `id`,`clothing_name`,`clothing_parts` FROM `catalog_clothing`");
        foreach (var row in data)
            _clothing.Add(row.Id, new(row.Id, row.ClothingName, row.PartIds));
    }

    public bool TryGetClothing(int itemId, out ClothingItem clothing) => _clothing.TryGetValue(itemId, out clothing);
}