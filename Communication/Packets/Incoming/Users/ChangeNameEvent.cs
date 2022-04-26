using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Utilities;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Users;

internal class ChangeNameEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IRoomManager _roomManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IDatabase _database;

    public ChangeNameEvent(IGameClientManager clientManager, IRoomManager roomManager, IAchievementManager achievementManager, IDatabase database)
    {
        _clientManager = clientManager;
        _roomManager = roomManager;
        _achievementManager = achievementManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
        if (user == null)
            return Task.CompletedTask;
        var newName = packet.PopString();
        var oldName = session.GetHabbo().Username;
        if (newName == oldName)
        {
            session.GetHabbo().ChangeName(oldName);
            session.SendPacket(new UpdateUsernameComposer(newName));
            return Task.CompletedTask;
        }
        if (!CanChangeName(session.GetHabbo()))
        {
            session.SendNotification("Oops, it appears you currently cannot change your username!");
            return Task.CompletedTask;
        }
        bool inUse;
        using (var connection = _database.Connection())
        {
            var checkUser = connection.ExecuteScalar<int>("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1", new { name = newName });
            inUse = checkUser == 1;
        }
        var letters = newName.ToLower().ToCharArray();
        const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";
        if (letters.Any(chr => !allowedCharacters.Contains(chr)))
            return Task.CompletedTask;
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && newName.ToLower().Contains("mod") || newName.ToLower().Contains("adm") || newName.ToLower().Contains("admin")
            || newName.ToLower().Contains("m0d") || newName.ToLower().Contains("mob") || newName.ToLower().Contains("m0b"))
            return Task.CompletedTask;
        if (!newName.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
            return Task.CompletedTask;
        if (newName.Length > 15)
            return Task.CompletedTask;
        if (newName.Length < 3)
            return Task.CompletedTask;
        if (inUse)
            return Task.CompletedTask;
        if (!_clientManager.UpdateClientUsername(session, oldName, newName))
        {
            session.SendNotification("Oops! An issue occoured whilst updating your username.");
            return Task.CompletedTask;
        }
        session.GetHabbo().ChangingName = false;
        room.GetRoomUserManager().RemoveUserFromRoom(session, true);
        session.GetHabbo().ChangeName(newName);
        session.GetHabbo().GetMessenger().OnStatusChanged(true);
        session.SendPacket(new UpdateUsernameComposer(newName));
        room.SendPacket(new UserNameChangeComposer(room.Id, user.VirtualId, newName));
        using (var connection = _database.Connection())
        {
            connection.Execute("INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES (@id,@new_name,@old_name,@timestamp)",
                    new { id = session.GetHabbo().Id, new_name = newName, old_name = oldName, timestamp = UnixTimestamp.GetNow() });
        }
        foreach (var ownRooms in _roomManager.GetRooms().ToList())
        {
            if (ownRooms == null || ownRooms.OwnerId != session.GetHabbo().Id || ownRooms.OwnerName == newName)
                continue;
            ownRooms.OwnerName = newName;
            ownRooms.SendPacket(new RoomInfoUpdatedComposer(ownRooms.Id));
        }
        _achievementManager.ProgressAchievement(session, "ACH_Name", 1);
        session.SendPacket(new RoomForwardComposer(room.Id));
        return Task.CompletedTask;
    }

    private static bool CanChangeName(Habbo habbo)
    {
        if (habbo.Rank == 1 && habbo.VipRank == 0 && habbo.LastNameChange == 0)
            return true;
        if (habbo.Rank == 1 && habbo.VipRank == 1 && (habbo.LastNameChange == 0 || UnixTimestamp.GetNow() + 604800 > habbo.LastNameChange))
            return true;
        if (habbo.Rank == 1 && habbo.VipRank == 2 && (habbo.LastNameChange == 0 || UnixTimestamp.GetNow() + 86400 > habbo.LastNameChange))
            return true;
        if (habbo.Rank == 1 && habbo.VipRank == 3)
            return true;
        if (habbo.GetPermissions().HasRight("mod_tool"))
            return true;
        return false;
    }
}