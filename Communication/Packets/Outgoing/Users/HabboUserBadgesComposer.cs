using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users;

internal class HabboUserBadgesComposer : ServerPacket
{
    public HabboUserBadgesComposer(Habbo habbo)
        : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
    {
        WriteInteger(habbo.Id);
        var badges = habbo.Inventory.Badges.EquippedBadges;
        WriteInteger(badges.Count());
        foreach (var badge in badges)
        {
            WriteInteger(badge.Slot);
            WriteString(badge.Code);
        }
    }
}