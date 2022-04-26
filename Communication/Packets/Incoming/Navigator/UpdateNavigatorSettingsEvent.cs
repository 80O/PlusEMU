using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class UpdateNavigatorSettingsEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var roomId = packet.PopInt();
        if (roomId == 0)
            return Task.CompletedTask;
        if (!RoomFactory.TryGetData(roomId, out var _))
            return Task.CompletedTask;
        session.GetHabbo().HomeRoom = roomId;
        session.SendPacket(new NavigatorSettingsComposer(roomId));
        return Task.CompletedTask;
    }
}