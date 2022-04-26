using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Core.Language;
using Plus.Database;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class AssignRightsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly ILanguageManager _languageManager;
    private readonly ICacheManager _chacheManager;
    private readonly IDatabase _database;

    public AssignRightsEvent(IRoomManager roomManager, ILanguageManager languageManager, ICacheManager cacheManager, IDatabase database)
    {
        _roomManager = roomManager;
        _languageManager = languageManager;
        _chacheManager = cacheManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        if (room.UsersWithRights.Contains(userId))
        {
            session.SendNotification(_languageManager.TryGetValue("room.rights.user.has_rights"));
            return Task.CompletedTask;
        }
        room.UsersWithRights.Add(userId);
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("INSERT INTO `room_rights` (`room_id`,`user_id`) VALUES ('" + room.RoomId + "','" + userId + "')");
        }
        var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
        if (roomUser != null && !roomUser.IsBot)
        {
            roomUser.SetStatus("flatctrl 1");
            roomUser.UpdateNeeded = true;
            if (roomUser.GetClient() != null)
                roomUser.GetClient().SendPacket(new YouAreControllerComposer(1));
            session.SendPacket(new FlatControllerAddedComposer(room.RoomId, roomUser.GetClient().GetHabbo().Id, roomUser.GetClient().GetHabbo().Username));
        }
        else
        {
            var user =  _chacheManager.GenerateUser(userId);
            if (user != null)
                session.SendPacket(new FlatControllerAddedComposer(room.RoomId, user.Id, user.Username));
        }
        return Task.CompletedTask;
    }
}