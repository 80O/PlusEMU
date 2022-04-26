using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class SetGroupFavouriteEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;

    public SetGroupFavouriteEvent(IGroupManager groupManager,IDatabase database)
    {
        _groupManager = groupManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        if (groupId == 0)
            return Task.CompletedTask;
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        session.GetHabbo().GetStats().FavouriteGroupId = group.Id;
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `user_stats` SET `groupid` = @groupId WHERE `id` = @userId LIMIT 1",
                new { groupId = session.GetHabbo().GetStats().FavouriteGroupId, userId = session.GetHabbo().Id });
        }
        if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null)
        {
            session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            session.GetHabbo().CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));
            var user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
                session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
        }
        else
            session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
        return Task.CompletedTask;
    }
}