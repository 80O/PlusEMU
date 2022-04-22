﻿using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    class GetRoomFilterListEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            var instance = session.GetHabbo().CurrentRoom;
            if (instance == null)
                return;

            if (!instance.CheckRights(session))
                return;

            session.SendPacket(new GetRoomFilterListComposer(instance));
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModRoomFilterSeen", 1);
        }
    }
}
