using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class TradingCancelConfirmEvent : IPacketEvent
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
            return Task.CompletedTask;
        trade.EndTrade(session.GetHabbo().Id);
        return Task.CompletedTask;
    }
}