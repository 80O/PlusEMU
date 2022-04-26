using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class FindNewFriendsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public FindNewFriendsEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var instance = _roomManager.TryGetRandomLoadedRoom();
        if (instance != null)
        {
            session.SendPacket(new FindFriendsProcessResultComposer(true));
            session.SendPacket(new RoomForwardComposer(instance.Id));
        }
        else
            session.SendPacket(new FindFriendsProcessResultComposer(false));
        return Task.CompletedTask;
    }
}