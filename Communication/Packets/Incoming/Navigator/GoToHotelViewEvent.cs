using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class GoToHotelViewEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public GoToHotelViewEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().InRoom)
        {
            if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var oldRoom))
                return Task.CompletedTask;
            if (oldRoom.GetRoomUserManager() != null)
                oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, true);
        }
        return Task.CompletedTask;
    }
}