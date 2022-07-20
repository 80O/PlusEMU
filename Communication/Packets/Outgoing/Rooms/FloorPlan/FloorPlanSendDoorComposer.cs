﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Rooms.FloorPlan;

public class FloorPlanSendDoorComposer : IServerPacket
{
    private readonly int _doorX;
    private readonly int _doorY;
    private readonly int _doorDirection;

    public uint MessageId => ServerPacketHeader.FloorPlanSendDoorComposer;

    public FloorPlanSendDoorComposer(int doorX, int doorY, int doorDirection)
    {
        _doorX = doorX;
        _doorY = doorY;
        _doorDirection = doorDirection;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_doorX);
        packet.WriteInteger(_doorY);
        packet.WriteInteger(_doorDirection);
    }
}