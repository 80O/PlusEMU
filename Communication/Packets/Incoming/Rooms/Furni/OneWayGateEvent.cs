using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class OneWayGateEvent : IPacketEvent
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
            if (item.GetBaseItem().InteractionType == InteractionType.OneWayGate)
            {
                item.Interactor.OnTrigger(session, item, -1, hasRights);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
