using Plus.Database;
using Dapper;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Users.Voucher;

public class UserVoucherManager : IUserVoucherManager
{
    readonly IDatabase _database;
    readonly IVoucherManager _voucherManager;

    public UserVoucherManager(IVoucherManager voucherManager, IDatabase database)
    {
        _voucherManager = voucherManager;
        _database = database;
    }

    async Task IUserVoucherManager.PacketHandler(GameClient session, string code)
    {
        if (!_voucherManager.TryGetVoucher(code, out var voucher))
        {
            session.Send(new VoucherRedeemErrorComposer(0));
            return;
        }

        if (voucher.CurrentUses >= voucher.MaxUses)
        {
            session.SendNotification("Oops, this voucher has reached the maximum usage limit!");
            return;
        }

        using var connection = _database.Connection();
        var voucherByUser = await connection.QueryFirstOrDefaultAsync<UserVoucher>("SELECT * FROM `user_vouchers` WHERE `user_id` = @userId AND `voucher` = @voucher LIMIT 1", new
        {
            userId = session.GetHabbo().Id,
            voucher = code
        });
        if (voucherByUser != null)
            session.SendNotification("You've already used this voucher code, one per each user, sorry!");
        else
            await connection.ExecuteAsync("INSERT INTO `user_vouchers` (`user_id`,`voucher`) VALUES (@userId, @voucher)", new
            {
                userId = session.GetHabbo().Id,
                voucher = code
            });

        voucher.UpdateUses();
        if (voucher.Type == VoucherType.Credit)
        {
            session.GetHabbo().Credits += voucher.Value;
            session.Send(new CreditBalanceComposer(session.GetHabbo().Credits));
        }
        else if (voucher.Type == VoucherType.Ducket)
        {
            session.GetHabbo().Duckets += voucher.Value;
            session.Send(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, voucher.Value));
        }
        session.Send(new VoucherRedeemOkComposer());
    }
}