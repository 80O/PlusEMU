﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine;

public class ObjectRemoveComposer : IServerPacket
{
    private readonly Item _item;
    private readonly int _userId;

    public uint MessageId => ServerPacketHeader.ObjectRemoveComposer;

    public ObjectRemoveComposer(Item item, int userId)
    {
        _item = item;
        _userId = userId;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteString(_item.Id.ToString());
        packet.WriteBoolean(false);
        packet.WriteInteger(_userId);
        packet.WriteInteger(0);
    }
}