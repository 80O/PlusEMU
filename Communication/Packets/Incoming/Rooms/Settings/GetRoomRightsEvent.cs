using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class GetRoomRightsEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var instance = session.GetHabbo().CurrentRoom;
        if (instance == null)
            return Task.CompletedTask;
        if (!instance.CheckRights(session))
            return Task.CompletedTask;
        session.SendPacket(new RoomRightsListComposer(instance));
        return Task.CompletedTask;
    }
}