using Plus.Utilities;

namespace Plus.HabboHotel.Rooms;

public class RoomPromotion
{
    public RoomPromotion(string name, string description, int categoryId)
    {
        Name = name;
        Description = description;
        TimestampStarted = UnixTimestamp.GetNow();
        TimestampExpires = UnixTimestamp.GetNow() + Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.promotion.lifespan")) * 60;
        CategoryId = categoryId;
    }

    public RoomPromotion(string name, string description, double started, double expires, int categoryId)
    {
        Name = name;
        Description = description;
        TimestampStarted = started;
        TimestampExpires = expires;
        CategoryId = categoryId;
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public double TimestampStarted { get; }

    public double TimestampExpires { get; set; }

    public bool HasExpired => TimestampExpires - UnixTimestamp.GetNow() < 0;

    public int MinutesLeft => Convert.ToInt32(Math.Ceiling((TimestampExpires - UnixTimestamp.GetNow()) / 60));

    public int CategoryId { get; set; }
}