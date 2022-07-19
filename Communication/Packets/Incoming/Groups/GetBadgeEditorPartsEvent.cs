﻿using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetBadgeEditorPartsEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public GetBadgeEditorPartsEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        session.Send(new BadgeEditorPartsComposer(
            _groupManager.BadgeBases,
            _groupManager.BadgeSymbols,
            _groupManager.BadgeBaseColours,
            _groupManager.BadgeSymbolColours,
            _groupManager.BadgeBackColours));
        return Task.CompletedTask;
    }
}