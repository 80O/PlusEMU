using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class MoveAvatarEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null || !user.CanWalk)
            return Task.CompletedTask;
        var moveX = packet.PopInt();
        var moveY = packet.PopInt();
        if (moveX == user.X && moveY == user.Y)
            return Task.CompletedTask;
        if (user.RidingHorse)
        {
            var horse = room.GetRoomUserManager().GetRoomUserByVirtualId(user.HorseId);
            if (horse != null)
                horse.MoveTo(moveX, moveY);
        }
        user.MoveTo(moveX, moveY);
        return Task.CompletedTask;
    }
}