using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class DropHandItemEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public DropHandItemEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        if (user.CarryItemId > 0 && user.CarryTimer > 0)
            user.CarryItem(0);
        return Task.CompletedTask;
    }
}