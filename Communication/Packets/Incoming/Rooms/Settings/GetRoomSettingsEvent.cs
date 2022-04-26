using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class GetRoomSettingsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public GetRoomSettingsEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var roomId = packet.PopInt();
        if (!_roomManager.TryLoadRoom(roomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        session.SendPacket(new RoomSettingsDataComposer(room));
        return Task.CompletedTask;
    }
}