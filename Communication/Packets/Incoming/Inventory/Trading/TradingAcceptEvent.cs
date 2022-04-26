using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class TradingAcceptEvent : IPacketEvent
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
        if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
        {
            session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        var tradeUser = trade.Users[0];
        if (tradeUser.RoomUser != roomUser)
            tradeUser = trade.Users[1];
        tradeUser.HasAccepted = true;
        trade.SendPacket(new TradingAcceptComposer(session.GetHabbo().Id, true));
        if (trade.AllAccepted)
        {
            trade.SendPacket(new TradingCompleteComposer());
            trade.CanChange = false;
            trade.RemoveAccepted();
        }
        return Task.CompletedTask;
    }
}