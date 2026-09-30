namespace Plus.HabboHotel.Catalog;

public sealed class CatalogClubOffer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public int Credits { get; set; }
    public int Points { get; set; }
    public int PointsType { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool Deal { get; set; }

    public bool IsVip => string.Equals(Type, "vip", StringComparison.OrdinalIgnoreCase);
    public int Months => Days / 31;
}
