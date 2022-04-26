using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class RemoveMyRightsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public RemoveMyRightsEvent(IRoomManager roomManager, IDatabase database)
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
        if (!room.CheckRights(session, false))
            return Task.CompletedTask;
        if (room.UsersWithRights.Contains(session.GetHabbo().Id))
        {
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null && !user.IsBot)
            {
                user.RemoveStatus("flatctrl 1");
                user.UpdateNeeded = true;
                user.GetClient().SendPacket(new YouAreNotControllerComposer());
            }
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `room_rights` WHERE `user_id` = @uid AND `room_id` = @rid LIMIT 1");
                dbClient.AddParameter("uid", session.GetHabbo().Id);
                dbClient.AddParameter("rid", room.Id);
                dbClient.RunQuery();
            }
            if (room.UsersWithRights.Contains(session.GetHabbo().Id))
                room.UsersWithRights.Remove(session.GetHabbo().Id);
        }
        return Task.CompletedTask;
    }
}