using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class EjectAllCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    private readonly IDatabase _database;
    public string Key => "ejectall";
    public string PermissionRequired => "command_ejectall";

    public string Parameters => "";

    public string Description => "Removes all of the items from the room.";

    public EjectAllCommand(IGameClientManager gameClientManager, IDatabase database)
    {
        _gameClientManager = gameClientManager;
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (session.GetHabbo().Id == room.OwnerId)
        {
            //Let us check anyway.
            if (!room.CheckRights(session, true))
                return;
            foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
            {
                if (item == null || item.UserId == session.GetHabbo().Id)
                    continue;
                var targetClient = _gameClientManager.GetClientByUserId(item.UserId);
                if (targetClient != null && targetClient.GetHabbo() != null)
                {
                    room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                    targetClient.GetHabbo().Inventory.AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    targetClient.SendPacket(new FurniListUpdateComposer());
                }
                else
                {
                    room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                    using var dbClient = _database.GetQueryReactor();
                    dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                }
            }
        }
        else
        {
            foreach (var item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
            {
                if (item == null || item.UserId != session.GetHabbo().Id)
                    continue;
                var targetClient = _gameClientManager.GetClientByUserId(item.UserId);
                if (targetClient != null && targetClient.GetHabbo() != null)
                {
                    room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                    targetClient.GetHabbo().Inventory.AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    targetClient.SendPacket(new FurniListUpdateComposer());
                }
                else
                {
                    room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                    using var dbClient = _database.GetQueryReactor();
                    dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                }
            }
        }
    }
}