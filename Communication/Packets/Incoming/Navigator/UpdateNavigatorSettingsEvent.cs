﻿using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class UpdateNavigatorSettingsEvent : IPacketEvent
{
    private readonly INavigatorManager _navigatorManager;

    public UpdateNavigatorSettingsEvent(NavigatorManager navigatorManager)
    {
        _navigatorManager = navigatorManager;
    }

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var roomId = packet.ReadInt();
        await _navigatorManager.SaveHomeRoom(session.GetHabbo(), roomId);
        session.Send(new NavigatorSettingsComposer(roomId));
    }
}