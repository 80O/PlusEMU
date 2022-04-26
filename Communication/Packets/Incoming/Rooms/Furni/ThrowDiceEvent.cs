using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class ThrowDiceEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
        if (item == null)
            return Task.CompletedTask;
        var hasRights = room.CheckRights(session, false, true);
        var request = packet.PopInt();
        item.Interactor.OnTrigger(session, item, request, hasRights);
        return Task.CompletedTask;
    }
}