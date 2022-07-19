﻿using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class FindRandomFriendingRoomEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public FindRandomFriendingRoomEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var instance = _roomManager.TryGetRandomLoadedRoom();
        if (instance != null)
            session.Send(new RoomForwardComposer(instance.Id));
        return Task.CompletedTask;
    }
}