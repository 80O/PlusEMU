using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading;

internal class InitTradeEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public InitTradeEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        
        var roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (roomUser == null)
            return Task.CompletedTask;
        var targetUser = room.GetRoomUserManager().GetRoomUserByVirtualId(userId);
        if (targetUser == null)
            return Task.CompletedTask;
        if (session.GetHabbo().TradingLockExpiry > 0)
        {
            if (session.GetHabbo().TradingLockExpiry > UnixTimestamp.GetNow())
            {
                session.SendNotification("You're currently banned from trading.");
                return Task.CompletedTask;
            }
            session.GetHabbo().TradingLockExpiry = 0;
            session.SendNotification("Your trading ban has now expired.");
            using var connection = _database.Connection();
            connection.Execute("UPDATE `user_info` SET `trading_locked` = '0' WHERE `id` = @userId LIMIT 1", new { userId = session.GetHabbo().Id });
        }
        if (!session.GetHabbo().GetPermissions().HasRight("room_trade_override"))
        {
            if (room.TradeSettings == 0)
            {
                session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                return Task.CompletedTask;
            }
            if (room.TradeSettings == 1 && room.OwnerId != session.GetHabbo().Id)
            {
                session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                return Task.CompletedTask;
            }
        }
        if (roomUser.IsTrading && roomUser.TradePartner != targetUser.UserId)
        {
            session.SendPacket(new TradingErrorComposer(7, targetUser.GetUsername()));
            return Task.CompletedTask;
        }
        if (targetUser.IsTrading && targetUser.TradePartner != roomUser.UserId)
        {
            session.SendPacket(new TradingErrorComposer(8, targetUser.GetUsername()));
            return Task.CompletedTask;
        }
        if (!targetUser.GetClient().GetHabbo().AllowTradingRequests)
        {
            session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
            return Task.CompletedTask;
        }
        if (targetUser.GetClient().GetHabbo().TradingLockExpiry > 0)
        {
            session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
            return Task.CompletedTask;
        }
        if (!room.GetTrading().StartTrade(roomUser, targetUser, out var trade))
        {
            session.SendNotification("An error occured trying to start this trade");
            return Task.CompletedTask;
        }
        if (targetUser.HasStatus("trd"))
            targetUser.RemoveStatus("trd");
        if (roomUser.HasStatus("trd"))
            roomUser.RemoveStatus("trd");
        targetUser.SetStatus("trd");
        targetUser.UpdateNeeded = true;
        roomUser.SetStatus("trd");
        roomUser.UpdateNeeded = true;
        trade.SendPacket(new TradingStartComposer(roomUser.UserId, targetUser.UserId));
        return Task.CompletedTask;
    }
}