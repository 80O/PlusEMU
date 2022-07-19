﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class NewBuddyRequestComposer : IServerPacket
{
    private readonly int _id;
    private readonly string _name;
    private readonly string _figure;
    public int MessageId => ServerPacketHeader.NewBuddyRequestMessageComposer;

    public NewBuddyRequestComposer(int id, string name, string figure)
    {
        _id = id;
        _name = name;
        _figure = figure;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_id);
        packet.WriteString(_name);
        packet.WriteString(_figure);
    }
}