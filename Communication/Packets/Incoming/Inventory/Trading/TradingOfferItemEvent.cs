using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class TradingOfferItemEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (roomUser == null)
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        if (!roomUser.IsTrading)
        {
            session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
        {
            session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        var item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);
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
            var totalLtDs = tradeUser.OfferedItems.Count(x => x.Value.LimitedNo > 0);
            if (totalLtDs < 9)
                tradeUser.OfferedItems.Add(item.Id, item);
        }
        trade.SendPacket(new TradingUpdateComposer(trade));
        return Task.CompletedTask;
    }
}