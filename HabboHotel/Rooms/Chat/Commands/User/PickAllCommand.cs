using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class PickAllCommand : IChatCommand
{
    private readonly IDatabase _database;
    public string Key => "pickall";
    public string PermissionRequired => "command_pickall";

    public string Parameters => "";

    public string Description => "Picks up all of the furniture from your room.";

    public PickAllCommand(IDatabase database)
    {
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (!room.CheckRights(session, true))
            return;
        room.GetRoomItemHandler().RemoveItems(session);
        room.GetGameMap().GenerateMaps();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `items` SET `room_id` = '0' WHERE `room_id` = @RoomId AND `user_id` = @UserId");
            dbClient.AddParameter("RoomId", room.Id);
            dbClient.AddParameter("UserId", session.GetHabbo().Id);
            dbClient.RunQuery();
        }
        var items = room.GetRoomItemHandler().GetWallAndFloor.ToList();
        if (items.Count > 0)
            session.SendWhisper("There are still more items in this room, manually remove them or use :ejectall to eject them!");
        session.SendPacket(new FurniListUpdateComposer());
    }
}