using System;
using System.Threading.Tasks;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class DeleteGroupEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;
    private readonly IRoomManager _roomManager;
    private readonly ISettingsManager _settingsManager;

    public DeleteGroupEvent(IGroupManager groupManager, IDatabase database, IRoomManager roomManager, ISettingsManager settingsManager)
    {
        _groupManager = groupManager;
        _database = database;
        _roomManager = roomManager;
        _settingsManager = settingsManager;
    }
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_groupManager.TryGetGroup(packet.PopInt(), out var group))
        {
            session.SendNotification("Oops, we couldn't find that group!");
            return Task.CompletedTask;
        }
        if (group.CreatorId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("group_delete_override")) //Maybe a FUSE check for staff override?
        {
            session.SendNotification("Oops, only the group owner can delete a group!");
            return Task.CompletedTask;
        }
        if (group.MemberCount >= Convert.ToInt32(_settingsManager.TryGetValue("group.delete.member.limit")) &&
            !session.GetHabbo().GetPermissions().HasRight("group_delete_limit_override"))
        {
            session.SendNotification("Oops, your group exceeds the maximum amount of members (" + Convert.ToInt32(_settingsManager.TryGetValue("group.delete.member.limit")) +
                                     ") a group can exceed before being eligible for deletion. Seek assistance from a staff member.");
            return Task.CompletedTask;
        }
        if (!_roomManager.TryGetRoom(group.RoomId, out var room))
            return Task.CompletedTask;
        if (!RoomFactory.TryGetData(group.RoomId, out var _))
            return Task.CompletedTask;
        room.Group = null;

        //Remove it from the cache.
        _groupManager.DeleteGroup(group.Id);

        //Now the :S stuff.
        using (var connection = _database.Connection())
        {
            connection.Execute("DELETE FROM `groups` WHERE `id` = @groupId", new { groupId = group.Id });
            connection.Execute("DELETE FROM `group_memberships` WHERE `group_id` = @groupId", new { groupId = group.Id });
            connection.Execute("DELETE FROM `group_requests` WHERE `group_id` = @groupId", new { groupId = group.Id });
            connection.Execute("UPDATE `rooms` SET `group_id` = 0 WHERE `group_id` = @groupId LIMIT 1", new { groupId = group.Id });
            connection.Execute("UPDATE `user_statistics` SET `groupid` = 0 WHERE `groupid` = @groupId LIMIT 1", new { groupId = group.Id });
            connection.Execute("DELETE FROM `items_groups` WHERE `group_id` = @groupId", new { groupId = group.Id });
        }

        //Unload it last.
        _roomManager.UnloadRoom(room.Id);

        //Say hey!
        session.SendNotification("You have successfully deleted your group.");
        return Task.CompletedTask;
    }
}