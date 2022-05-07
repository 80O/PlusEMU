﻿using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Ignores;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetIgnoredUsersEvent : IPacketEvent
{
    private readonly IIgnoresManager _ignoresComponent;

    public GetIgnoredUsersEvent(IIgnoresManager ignoresComponent) => _ignoresComponent = ignoresComponent;

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var ignoredUsers = _ignoresComponent.GetIgnoredUsers(session.GetHabbo());
        session.SendPacket(new IgnoredUsersComposer(ignoredUsers));
        return Task.CompletedTask;
    }
}