﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.LandingView;

internal class ConcurrentUsersGoalProgressComposer : IServerPacket
{
    private readonly int _usersNow;
    public int MessageId => ServerPacketHeader.ConcurrentUsersGoalProgressMessageComposer;

    public ConcurrentUsersGoalProgressComposer(int usersNow)
    {
        _usersNow = usersNow;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(0); //0/1 = Not done, 2 = Done & can claim, 3 = claimed.
        packet.WriteInteger(_usersNow);
        packet.WriteInteger(1000);
    }
}