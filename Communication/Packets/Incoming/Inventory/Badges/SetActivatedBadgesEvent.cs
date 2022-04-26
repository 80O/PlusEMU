using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Inventory.Badges;

internal class SetActivatedBadgesEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;

    public SetActivatedBadgesEvent(IQuestManager questManager, IRoomManager roomManager, IDatabase database)
    {
        _questManager = questManager;
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.GetHabbo().GetBadgeComponent().ResetSlots();
        using (var connection = _database.Connection())
        {
            connection.Execute("UPDATE `user_badges` SET `badge_slot` = '0' WHERE `user_id` = @userId",
                    new { userId = session.GetHabbo().Id });
        }
        for (var i = 0; i < 5; i++)
        {
            var slot = packet.PopInt();
            var badge = packet.PopString();
            if (badge.Length == 0)
                continue;
            if (!session.GetHabbo().GetBadgeComponent().HasBadge(badge) || slot < 1 || slot > 5)
                return Task.CompletedTask;
            session.GetHabbo().GetBadgeComponent().GetBadge(badge).Slot = slot;
            using var connection = _database.Connection();

            connection.Execute("UPDATE `user_badges` SET `badge_slot` = @slot WHERE `badge_id` = @badge AND `user_id` = @userId LIMIT 1",
                        new { slot = slot, badge = badge, userId = session.GetHabbo().Id });
        }
        _questManager.ProgressUserQuest(session, QuestType.ProfileBadge);
        if (session.GetHabbo().InRoom && _roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            room.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
        else
            session.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
        return Task.CompletedTask;
    }
}