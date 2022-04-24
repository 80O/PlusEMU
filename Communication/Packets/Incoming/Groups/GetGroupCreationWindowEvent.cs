﻿using System.Linq;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetGroupCreationWindowEvent : IPacketEvent
{
    public void Parse(GameClient session, ClientPacket packet)
    {
        var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id).Where(x => x.Group == null).ToList();
        session.SendPacket(new GroupCreationWindowComposer(rooms));
    }
}