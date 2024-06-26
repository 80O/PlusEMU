﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Help;

public class SubmitBullyReportComposer : IServerPacket
{
    private readonly int _result;
    public uint MessageId => ServerPacketHeader.SubmitBullyReportComposer;

    public SubmitBullyReportComposer(int result)
    {
        _result = result;
    }

    public void Compose(IOutgoingPacket packet) => packet.WriteInteger(_result);
}