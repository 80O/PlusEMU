﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Users;

public class NameChangeUpdateComposer : IServerPacket
{
    private readonly string _name;
    private readonly int _error;
    private readonly ICollection<string> _tags;

    public uint MessageId => ServerPacketHeader.NameChangeUpdateComposer;

    public NameChangeUpdateComposer(string name, int error, ICollection<string> tags)
    {
        _name = name;
        _error = error;
        _tags = tags;
    }

    public NameChangeUpdateComposer(string name, int error)
    {
        _name = name;
        _error = error;
        _tags = Array.Empty<string>();
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_error);
        packet.WriteString(_name);
        packet.WriteInteger(_tags.Count);
        foreach (var tag in _tags) packet.WriteString(_name + tag);
    }
}