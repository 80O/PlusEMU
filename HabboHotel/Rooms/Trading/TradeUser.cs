using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Trading;

public sealed class TradeUser
{
    public TradeUser(RoomUser user)
    {
        RoomUser = user;
        HasAccepted = false;
        OfferedItems = new Dictionary<int, Item>();
    }

    public RoomUser RoomUser { get; }

    public bool HasAccepted { get; set; }

    public Dictionary<int, Item> OfferedItems { get; set; }
}