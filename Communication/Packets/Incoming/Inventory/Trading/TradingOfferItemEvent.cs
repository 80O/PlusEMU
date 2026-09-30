using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class TradingOfferItemEvent : IPacketEvent
{
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (roomUser == null)
            return Task.CompletedTask;
        var itemId = packet.ReadUInt();
        if (!roomUser.IsTrading)
        {
            session.Send(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
        {
            session.Send(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        lock (trade.SyncRoot)
        {
            var item = session.GetHabbo().Inventory.Furniture.GetItem(itemId);
            if (item == null)
                return Task.CompletedTask;
            if (!trade.CanChange)
                return Task.CompletedTask;
            var tradeUser = trade.Users[0];
            if (tradeUser.RoomUser != roomUser)
                tradeUser = trade.Users[1];
            if (tradeUser.OfferedItems.ContainsKey(item.Id))
                return Task.CompletedTask;
            trade.RemoveAccepted();
            if (tradeUser.OfferedItems.Count <= 499)
            {
                var totalLtDs = tradeUser.OfferedItems.Count(x => x.Value.UniqueNumber > 0);
                if (totalLtDs < 9)
                    tradeUser.OfferedItems.Add(item.Id, item);
            }
            trade.SendPacket(new TradingUpdateComposer(trade));
        }
        return Task.CompletedTask;
    }
}
