using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class RemoveRightsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public RemoveRightsEvent(IRoomManager roomManager, IDatabase database)
    {
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        var amount = packet.PopInt();
        for (var i = 0; i < amount && i <= 100; i++)
        {
            var userId = packet.PopInt();
            if (userId > 0 && room.UsersWithRights.Contains(userId))
            {
                var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null && !user.IsBot)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;
                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                using (var dbClient = _database.GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                    dbClient.AddParameter("uid", userId);
                    dbClient.AddParameter("rid", room.Id);
                    dbClient.RunQuery();
                }
                if (room.UsersWithRights.Contains(userId))
                    room.UsersWithRights.Remove(userId);
                session.SendPacket(new FlatControllerRemovedComposer(room, userId));
            }
        }
        return Task.CompletedTask;
    }
}