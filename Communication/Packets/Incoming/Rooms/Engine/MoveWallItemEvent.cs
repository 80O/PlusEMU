using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class MoveWallItemEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public MoveWallItemEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var wallPositionData = packet.PopString();
        var item = room.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        try
        {
            var wallPos = room.GetRoomItemHandler().WallPositionCheck(":" + wallPositionData.Split(':')[1]);
            item.WallCoord = wallPos;
        }
        catch
        {
            return Task.CompletedTask;
        }
        room.GetRoomItemHandler().UpdateItem(item);
        room.SendPacket(new ItemUpdateComposer(item, room.OwnerId));
        return Task.CompletedTask;
    }
}