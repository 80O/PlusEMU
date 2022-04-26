using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateGroupSettingsEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public UpdateGroupSettingsEvent(IGroupManager groupManager, IRoomManager roomManager, IDatabase database)
    {
        _groupManager = groupManager;
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id)
            return Task.CompletedTask;
        var type = packet.PopInt();
        var furniOptions = packet.PopInt();
        switch (type)
        {
            default:
                group.Type = GroupType.Open;
                break;
            case 1:
                group.Type = GroupType.Locked;
                break;
            case 2:
                group.Type = GroupType.Private;
                break;
        }
        if (group.Type != GroupType.Locked)
        {
            if (group.GetRequests.Count > 0)
            {
                foreach (var userId in group.GetRequests.ToList()) group.HandleRequest(userId, false);
                group.ClearRequests();
            }
        }
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `groups` SET `state` = @groupState, `admindeco` = @adminDeco WHERE `id` = @groupId LIMIT 1",
                new { groupState = (group.Type == GroupType.Open ? 0 : group.Type == GroupType.Locked ? 1 : 2).ToString(), adminDeco = (furniOptions == 1 ? 1 : 0).ToString(), groupId = group.Id });
        }
        group.AdminOnlyDeco = furniOptions;
        if (!_roomManager.TryGetRoom(group.RoomId, out var room))
            return Task.CompletedTask;
        foreach (var user in room.GetRoomUserManager().GetRoomUsers().ToList())
        {
            if (room.OwnerId == user.UserId || group.IsAdmin(user.UserId) || !group.IsMember(user.UserId))
                continue;
            if (furniOptions == 1)
            {
                user.RemoveStatus("flatctrl 1");
                user.UpdateNeeded = true;
                user.GetClient().SendPacket(new YouAreControllerComposer(0));
            }
            else if (furniOptions == 0 && !user.Statusses.ContainsKey("flatctrl 1"))
            {
                user.SetStatus("flatctrl 1");
                user.UpdateNeeded = true;
                user.GetClient().SendPacket(new YouAreControllerComposer(1));
            }
        }
        session.SendPacket(new GroupInfoComposer(group, session));
        return Task.CompletedTask;
    }
}