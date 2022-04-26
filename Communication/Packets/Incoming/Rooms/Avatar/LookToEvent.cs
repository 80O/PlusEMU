using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class LookToEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public LookToEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        if (user.IsAsleep)
            return Task.CompletedTask;
        user.UnIdle();
        var x = packet.PopInt();
        var y = packet.PopInt();
        if (x == user.X && y == user.Y || user.IsWalking || user.RidingHorse)
            return Task.CompletedTask;
        var rot = Rotation.Calculate(user.X, user.Y, x, y);
        user.SetRot(rot, false);
        user.UpdateNeeded = true;
        if (user.RidingHorse)
        {
            var horse = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByVirtualId(user.HorseId);
            if (horse != null)
            {
                horse.SetRot(rot, false);
                horse.UpdateNeeded = true;
            }
        }
        return Task.CompletedTask;
    }
}