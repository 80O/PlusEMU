using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class UseFurnitureEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public UseFurnitureEvent(IRoomManager roomManager, IQuestManager questManager, IDatabase database)
    {
        _roomManager = roomManager;
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        var hasRights = room.CheckRights(session, false, true);
        if (item.GetBaseItem().InteractionType == InteractionType.Banzaitele)
            return Task.CompletedTask;
        if (item.GetBaseItem().InteractionType == InteractionType.Toner)
        {
            if (!room.CheckRights(session, true))
                return Task.CompletedTask;
            room.TonerData.Enabled = room.TonerData.Enabled == 0 ? 1 : 0;
            room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            item.UpdateState();
            using var dbClient = _database.GetQueryReactor();
            dbClient.RunQuery("UPDATE `room_items_toner` SET `enabled` = '" + room.TonerData.Enabled + "' LIMIT 1");
            return Task.CompletedTask;
        }
        if (item.Data.InteractionType == InteractionType.GnomeBox && item.UserId == session.GetHabbo().Id) session.SendPacket(new GnomeBoxComposer(item.Id));
        var toggle = true;
        if (item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch1 || item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch2)
        {
            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return Task.CompletedTask;
            if (!Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y)) toggle = false;
        }
        var request = packet.PopInt();
        item.Interactor.OnTrigger(session, item, request, hasRights);
        if (toggle)
            item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
        _questManager.ProgressUserQuest(session, QuestType.ExploreFindItem, item.GetBaseItem().Id);
        return Task.CompletedTask;
    }
}