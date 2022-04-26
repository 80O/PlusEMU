using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class ToggleMuteToolEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null || !room.CheckRights(session, true))
            return Task.CompletedTask;
        room.RoomMuted = !room.RoomMuted;
        var roomUsers = room.GetRoomUserManager().GetRoomUsers();
        foreach (var roomUser in roomUsers.ToList())
        {
            if (roomUser == null || roomUser.GetClient() == null)
                continue;
            roomUser.GetClient()
                .SendWhisper(room.RoomMuted ? "This room has been muted" : "This room has been unmuted");
        }
        room.SendPacket(new RoomMuteSettingsComposer(room.RoomMuted));
        return Task.CompletedTask;
    }
}