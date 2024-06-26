﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public class PurchaseErrorComposer : IServerPacket
{
    private readonly int _errorCode;
    public uint MessageId => ServerPacketHeader.PurchaseErrorComposer;

    public PurchaseErrorComposer(int errorCode)
    {
        // TODO @80O: Convert to enum
        _errorCode = errorCode;
    }

    public void Compose(IOutgoingPacket packet) => packet.WriteInteger(_errorCode);
}