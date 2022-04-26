using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class LetUserInEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IGameClientManager _clientManager;

    public LetUserInEvent(IRoomManager roomManager, IGameClientManager clientManager)
    {
        _roomManager = roomManager;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session))
            return Task.CompletedTask;
        var name = packet.PopString();
        var accepted = packet.PopBoolean();
        var client = _clientManager.GetClientByUsername(name);
        if (client == null)
            return Task.CompletedTask;
        if (accepted)
        {
            client.GetHabbo().RoomAuthOk = true;
            client.SendPacket(new FlatAccessibleComposer(""));
            room.SendPacket(new FlatAccessibleComposer(client.GetHabbo().Username), true);
        }
        else
        {
            client.SendPacket(new FlatAccessDeniedComposer(""));
            room.SendPacket(new FlatAccessDeniedComposer(client.GetHabbo().Username), true);
        }
        return Task.CompletedTask;
    }
}