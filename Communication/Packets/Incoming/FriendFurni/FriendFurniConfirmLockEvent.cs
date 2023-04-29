using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.FriendFurni;

internal class FriendFurniConfirmLockEvent : IPacketEvent
{
    private readonly IItemDataManager _itemDataManager;

    public FriendFurniConfirmLockEvent(IItemDataManager itemDataManager) =>
        _itemDataManager = itemDataManager;

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var pId = packet.ReadUInt();
        var isConfirmed = packet.ReadBool();

        await _itemDataManager.ConfirmLockEvent(session, pId, isConfirmed);
    }
}