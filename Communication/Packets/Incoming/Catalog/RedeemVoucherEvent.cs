using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Voucher;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class RedeemVoucherEvent : IPacketEvent
{
    readonly IUserVoucherManager _userVoucherManager;

    public RedeemVoucherEvent(IUserVoucherManager userVoucherManager) =>
        _userVoucherManager = userVoucherManager;

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var code = packet.ReadString().Replace("\r", "");
        await _userVoucherManager.PacketHandler(session, code);
    }
}