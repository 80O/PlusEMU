namespace Plus.HabboHotel.Users.Voucher;

public interface IUserVoucherManager
{
    Task<UserVoucher?> GetVoucherByUserIdAndCode(int userId, string code);

    Task CreateVoucher(int userId, string code);
}
