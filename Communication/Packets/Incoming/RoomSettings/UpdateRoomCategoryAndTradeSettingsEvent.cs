﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.RoomSettings;

internal class UpdateRoomCategoryAndTradeSettingsEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly INavigatorManager _navigationManager;

    public UpdateRoomCategoryAndTradeSettingsEvent(IRoomManager roomManager, INavigatorManager navigatorManager)
    {
        _roomManager = roomManager;
        _navigationManager = navigatorManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!_roomManager.TryGetRoom(packet.ReadInt(), out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        var categoryId = packet.ReadInt();
        var tradeSettings = packet.ReadInt();
        if (tradeSettings < 0 || tradeSettings > 2)
            tradeSettings = 0;
        if (!_navigationManager.TryGetSearchResultList(categoryId, out var searchResultList)) categoryId = 36;
        if (searchResultList.CategoryType != NavigatorCategoryType.Category || searchResultList.RequiredRank > session.GetHabbo().Rank) categoryId = 36;
        return Task.CompletedTask;
    }
}