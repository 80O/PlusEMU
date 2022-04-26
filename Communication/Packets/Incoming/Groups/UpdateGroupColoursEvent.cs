using System;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateGroupColoursEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;

    public UpdateGroupColoursEvent(IGroupManager groupManager, IDatabase database)
    {
        _groupManager = groupManager;
        _database = database;
    }
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        var mainColour = packet.PopInt();
        var secondaryColour = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        if (group.CreatorId != session.GetHabbo().Id)
            return Task.CompletedTask;
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = @groupId LIMIT 1",
                new { colour1 = mainColour, colour2 = secondaryColour, groupId = group.Id });
        }
        group.Colour1 = mainColour;
        group.Colour2 = secondaryColour;
        session.SendPacket(new GroupInfoComposer(group, session));
        if (session.GetHabbo().CurrentRoom != null)
        {
            foreach (var item in session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
            {
                if (item == null || item.GetBaseItem() == null)
                    continue;
                if (item.GetBaseItem().InteractionType != InteractionType.GuildItem && item.GetBaseItem().InteractionType != InteractionType.GuildGate ||
                    item.GetBaseItem().InteractionType != InteractionType.GuildForum)
                    continue;
                session.GetHabbo().CurrentRoom.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(item.UserId)));
            }
        }
        return Task.CompletedTask;
    }
}