﻿using System.Linq;

using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users
{
    class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo habbo)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            WriteInteger(habbo.Id);
            WriteInteger(habbo.GetBadgeComponent().EquippedCount);

            foreach (var badge in habbo.GetBadgeComponent().GetBadges().Where(b => b.Slot > 0).ToList())
            {
                WriteInteger(badge.Slot);
                WriteString(badge.Code);
            }
        }
    }
}
