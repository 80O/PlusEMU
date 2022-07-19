﻿using System.Globalization;
using System.Text;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine;

internal class UserUpdateComposer : IServerPacket
{
    private readonly ICollection<RoomUser> _users;
    public int MessageId => ServerPacketHeader.UserUpdateMessageComposer;

    public UserUpdateComposer(ICollection<RoomUser> users)
    {
        _users = users;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_users.Count);
        foreach (var user in _users.ToList())
        {
            packet.WriteInteger(user.VirtualId);
            packet.WriteInteger(user.X);
            packet.WriteInteger(user.Y);
            packet.WriteString(user.Z.ToString(CultureInfo.InvariantCulture));
            packet.WriteInteger(user.RotHead);
            packet.WriteInteger(user.RotBody);
            var statusComposer = new StringBuilder();
            statusComposer.Append("/");
            foreach (var status in user.Statusses.ToList())
            {
                statusComposer.Append(status.Key);
                if (!string.IsNullOrEmpty(status.Value))
                {
                    statusComposer.Append(" ");
                    statusComposer.Append(status.Value);
                }
                statusComposer.Append("/");
            }
            statusComposer.Append("/");
            packet.WriteString(statusComposer.ToString());
        }
    }
}