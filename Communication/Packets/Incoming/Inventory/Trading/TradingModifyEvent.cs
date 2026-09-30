using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class TradingModifyEvent : IPacketEvent
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
        if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out var trade))
        {
            session.Send(new TradingClosedComposer(session.GetHabbo().Id));
            return Task.CompletedTask;
        }
        lock (trade.SyncRoot)
        {
            if (!trade.CanChange)
                return Task.CompletedTask;
            var user = trade.Users[0];
            if (user.RoomUser != roomUser)
                user = trade.Users[1];
            user.HasAccepted = false;
            trade.SendPacket(new TradingAcceptComposer(session.GetHabbo().Id, false));
        }
        return Task.CompletedTask;
    }
}
