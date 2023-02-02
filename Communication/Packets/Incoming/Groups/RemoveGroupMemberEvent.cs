using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class RemoveGroupMemberEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;
    private readonly ICacheManager _cacheManager;

    public RemoveGroupMemberEvent(IGroupManager groupManager, IRoomManager roomManager, IDatabase database, ICacheManager cacheManager)
    {
        _groupManager = groupManager;
        _roomManager = roomManager;
        _database = database;
        _cacheManager = cacheManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var groupId = packet.ReadInt();
        var userId = packet.ReadInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (userId == session.GetHabbo().Id)
        {
            if (group.IsMember(userId))
                group.DeleteMember(userId);
            if (group.IsAdmin(userId))
            {
                if (group.IsAdmin(userId))
                    group.TakeAdmin(userId);
                if (!_roomManager.TryGetRoom(group.RoomId, out var room))
                    return Task.CompletedTask;
                var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;
                    if (user.GetClient() != null)
                        user.GetClient().Send(new YouAreControllerComposer(0));
                }
            }
            using (var connection = _database.Connection())
            {
                connection.Execute(
                    "DELETE FROM `group_memberships` WHERE `group_id` = @groupId AND `user_id` = @userId", new { groupId = groupId, userId = userId });
            }
            session.Send(new GroupInfoComposer(group, session));
            if (session.GetHabbo().HabboStats.FavouriteGroupId == groupId)
            {
                session.GetHabbo().HabboStats.FavouriteGroupId = 0;
                using (var connection = _database.Connection())
                {
                    connection.Execute("UPDATE `user_statistics` SET `groupid` = '0' WHERE `id` = @userId LIMIT 1", new { userId = userId });
                }
                if (group.AdminOnlyDeco == 0)
                {
                    if (!_roomManager.TryGetRoom(group.RoomId, out var room))
                        return Task.CompletedTask;
                    var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                    if (user != null)
                    {
                        user.RemoveStatus("flatctrl 1");
                        user.UpdateNeeded = true;
                        if (user.GetClient() != null)
                            user.GetClient().Send(new YouAreControllerComposer(0));
                    }
                }
                if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null)
                {
                    var user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                        .GetRoomUserByHabbo(session.GetHabbo().Id);
                    if (user != null)
                    {
                        session.GetHabbo().CurrentRoom
                            .SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                    }
                    session.GetHabbo().CurrentRoom
                        .SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }
                else
                    session.Send(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            }
            return Task.CompletedTask;
        }
        if (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id))
        {
            if (!group.IsMember(userId))
                return Task.CompletedTask;
            if (group.IsAdmin(userId) && group.CreatorId != session.GetHabbo().Id)
            {
                session.SendNotification(
                    "Sorry, only group creators can remove other administrators from the group.");
                return Task.CompletedTask;
            }
            if (group.IsAdmin(userId))
                group.TakeAdmin(userId);
            if (group.IsMember(userId))
                group.DeleteMember(userId);
            var members = new List<CachedUser>();
            var memberIds = group.GetAllMembers;
            foreach (var id in memberIds.ToList())
            {
                var groupMember = _cacheManager.GenerateUser(id);
                if (groupMember == null)
                    continue;
                if (!members.Contains(groupMember))
                    members.Add(groupMember);
            }
            var finishIndex = 14 < members.Count ? 14 : members.Count;
            var membersCount = members.Count;
            session.Send(new GroupMembersComposer(group, members.Take(finishIndex).ToList(), membersCount, 1,
                group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id), 0, ""));
        }
        return Task.CompletedTask;
    }
}