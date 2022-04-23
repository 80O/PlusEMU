using Plus.HabboHotel.GameClients;
using Plus.Database;


namespace Plus.HabboHotel.Items.Interactor;

public class InteractorHopper : IFurniInteractor
{
    private readonly IDatabase _database;

    public InteractorHopper(IDatabase database)
    {
        _database = database;
    }
    public void OnPlace(GameClient session, Item item)
    {
        item.GetRoom().GetRoomItemHandler().HopperCount++;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("INSERT INTO items_hopper (hopper_id, room_id) VALUES (@hopperid, @roomid);");
            dbClient.AddParameter("hopperid", item.Id);
            dbClient.AddParameter("roomid", item.RoomId);
            dbClient.RunQuery();
        }
        if (item.InteractingUser != 0)
        {
            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
            if (user != null)
            {
                user.ClearMovement(true);
                user.AllowOverride = false;
                user.CanWalk = true;
            }
            item.InteractingUser = 0;
        }
    }

    public void OnRemove(GameClient session, Item item)
    {
        item.GetRoom().GetRoomItemHandler().HopperCount--;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("DELETE FROM items_hopper WHERE item_id=@hid OR room_id=" + item.GetRoom().RoomId +
                              " LIMIT 1");
            dbClient.AddParameter("hid", item.Id);
            dbClient.RunQuery();
        }
        if (item.InteractingUser != 0)
        {
            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
            if (user != null) user.UnlockWalking();
            item.InteractingUser = 0;
        }
    }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (item == null || item.GetRoom() == null || session == null || session.GetHabbo() == null)
            return;
        var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null) return;

        // Alright. But is this user in the right position?
        if (user.Coordinate == item.Coordinate || user.Coordinate == item.SquareInFront)
        {
            // Fine. But is this tele even free?
            if (item.InteractingUser != 0) return;
            user.TeleDelay = 2;
            item.InteractingUser = user.GetClient().GetHabbo().Id;
        }
        else if (user.CanWalk) user.MoveTo(item.SquareInFront);
    }

    public void OnWiredTrigger(Item item) { }
}