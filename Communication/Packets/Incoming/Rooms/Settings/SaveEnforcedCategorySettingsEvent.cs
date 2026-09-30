using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings;

internal class SaveEnforcedCategorySettingsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly INavigatorManager _navigationManager;

    public SaveEnforcedCategorySettingsEvent(IRoomManager roomManager, INavigatorManager navigatorManager)
    {
        _roomManager = roomManager;
        _navigationManager = navigatorManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!_roomManager.TryGetRoom(packet.ReadUInt(), out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        var categoryId = packet.ReadInt();
        var tradeSettings = packet.ReadInt();
        if (tradeSettings < 0 || tradeSettings > 2)
            tradeSettings = 0;
        if (!_navigationManager.TryGetSearchResultList(categoryId, out var searchResultList) ||
            searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank)
            categoryId = 36;
        return Task.CompletedTask;
    }
}
