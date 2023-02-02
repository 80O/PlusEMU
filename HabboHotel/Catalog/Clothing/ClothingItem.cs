namespace Plus.HabboHotel.Catalog.Clothing;

public class ClothingItem
{
    public ClothingItem(int id, string name, string partIds)
    {
        Id = id;
        ClothingName = name;
        PartIds = partIds.Split(",").Select(int.Parse).ToList();
    }

    public int Id { get; }
    public string ClothingName { get; }
    public List<int> PartIds { get; }
}