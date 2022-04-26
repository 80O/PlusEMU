using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class DiceOffEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(packet.PopInt());
        if (item == null)
            return Task.CompletedTask;
        var hasRights = room.CheckRights(session);
        item.Interactor.OnTrigger(session, item, -1, hasRights);
        return Task.CompletedTask;
    }
}