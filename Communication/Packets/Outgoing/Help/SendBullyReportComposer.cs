﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Help;

// TODO @80O: Implement
public class SendBullyReportComposer : IServerPacket
{
    public int MessageId => ServerPacketHeader.SendBullyReportMessageComposer;

    public void Compose(IOutgoingPacket packet) => packet.WriteInteger(0); //0-3, sends 0 on Habbo for this purpose.
}