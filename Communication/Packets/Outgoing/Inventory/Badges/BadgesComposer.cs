using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Badges;

namespace Plus.Communication.Packets.Outgoing.Inventory.Badges;

internal class BadgesComposer : ServerPacket
{
    public BadgesComposer(GameClient session)
        : base(ServerPacketHeader.BadgesMessageComposer)
    {
        var equippedBadges = new List<Badge>();
        var badges = session.GetHabbo().Inventory.Badges.Badges;
        WriteInteger(badges.Count);
        foreach (var (_, badge) in badges)
        {
            WriteInteger(1);
            WriteString(badge.Code);
            if (badge.Slot > 0)
                equippedBadges.Add(badge);
        }
        WriteInteger(equippedBadges.Count);
        foreach (var badge in equippedBadges)
        {
            WriteInteger(badge.Slot);
            WriteString(badge.Code);
        }
    }
}