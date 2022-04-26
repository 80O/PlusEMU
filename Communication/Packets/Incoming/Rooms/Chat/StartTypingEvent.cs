using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat;

public class StartTypingEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
        if (user == null)
            return Task.CompletedTask;
        session.GetHabbo().CurrentRoom.SendPacket(new UserTypingComposer(user.VirtualId, true));
        return Task.CompletedTask;
    }
}