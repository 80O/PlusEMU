namespace Plus.HabboHotel.Cache.Type;

public class CachedUser
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Motto { get; init; } = string.Empty;
    public string Look { get; init; } = string.Empty;
    public DateTime AddedTime { get; set; } = DateTime.UtcNow;

    public bool IsExpired => (DateTime.UtcNow - AddedTime).TotalMinutes >= 30;
}