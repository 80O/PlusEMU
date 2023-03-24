using Plus.Database;
using Dapper;

namespace Plus.HabboHotel.Users.Voucher;

public class UserVoucherManager : IUserVoucherManager
{
    readonly IDatabase _database;

    public UserVoucherManager(IDatabase database) => _database = database;

    async Task IUserVoucherManager.CreateVoucher(int userId, string code)
    {
        using var connection = _database.Connection();
        await connection.ExecuteAsync("INSERT INTO `user_vouchers` (`user_id`,`voucher`) VALUES (@userId, @voucher)", new
        {
            userId,
            voucher = code
        });
    }

    async Task<UserVoucher?> IUserVoucherManager.GetVoucherByUserIdAndCode(int userId, string code)
    {
        using var connection = _database.Connection();
        return await connection.QueryFirstOrDefaultAsync<UserVoucher>("SELECT * FROM `user_vouchers` WHERE `user_id` = @userId AND `voucher` = @voucher LIMIT 1", new
        {
            userId,
            voucher = code
        });
    }
}