using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Users.Voucher;

public interface IUserVoucherManager
{
    Task PacketHandler(GameClient client, string code);
}