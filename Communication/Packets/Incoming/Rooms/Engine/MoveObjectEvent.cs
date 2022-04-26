using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class MoveObjectEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IQuestManager _questManager;

    public MoveObjectEvent(IRoomManager roomManager, IQuestManager questManager)
    {
        _roomManager = roomManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        if (itemId == 0)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        Item item;
        if (room.Group != null)
        {
            if (!room.CheckRights(session, false, true))
            {
                item = room.GetRoomItemHandler().GetItem(itemId);
                if (item == null)
                    return Task.CompletedTask;
                session.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
                return Task.CompletedTask;
            }
        }
        else
        {
            if (!room.CheckRights(session)) return Task.CompletedTask;
        }
        item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        var x = packet.PopInt();
        var y = packet.PopInt();
        var rotation = packet.PopInt();
        if (x != item.GetX || y != item.GetY)
            _questManager.ProgressUserQuest(session, QuestType.FurniMove);
        if (rotation != item.Rotation)
            _questManager.ProgressUserQuest(session, QuestType.FurniRotate);
        if (!room.GetRoomItemHandler().SetFloorItem(session, item, x, y, rotation, false, false, true))
        {
            room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            return Task.CompletedTask;
        }
        if (item.GetZ >= 0.1)
            _questManager.ProgressUserQuest(session, QuestType.FurniStack);
        return Task.CompletedTask;
    }
}