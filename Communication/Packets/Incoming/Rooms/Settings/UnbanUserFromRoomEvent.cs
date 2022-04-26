using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class UnbanUserFromRoomEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null || !instance.CheckRights(session, true))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        var roomId = packet.PopInt();
        if (instance.GetBans().IsBanned(userId))
        {
            instance.GetBans().Unban(userId);
            session.SendPacket(new UnbanUserFromRoomComposer(roomId, userId));
        }
        return Task.CompletedTask;
    }
}