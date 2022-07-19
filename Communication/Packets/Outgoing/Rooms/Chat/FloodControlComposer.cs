﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Rooms.Chat;

public class FloodControlComposer : IServerPacket
{
    private readonly int _floodTime;

    public int MessageId => ServerPacketHeader.FloodControlMessageComposer;

    public FloodControlComposer(int floodTime)
    {
        _floodTime = floodTime;
    }

    public void Compose(IOutgoingPacket packet) => packet.WriteInteger(_floodTime);
}