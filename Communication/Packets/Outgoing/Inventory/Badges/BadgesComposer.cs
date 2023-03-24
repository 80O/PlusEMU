using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Badges;

namespace Plus.Communication.Packets.Outgoing.Inventory.Badges;

public class BadgesComposer : IServerPacket
{
    private readonly int _userId;
    private readonly IReadOnlyDictionary<string, Badge> _badges;
    public uint MessageId => ServerPacketHeader.BadgesComposer;

    public BadgesComposer(int userId, IReadOnlyDictionary<string, Badge> badges)
    {
        _userId = userId;
        _badges = badges;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var equippedBadges = _badges.Values.Where(badge => badge.Slot > 0).OrderBy(badge => badge.Slot).ToList();

        packet.WriteInteger(_badges.Count);
        foreach (var badge in _badges.Values)
        {
            packet.WriteInteger(1);
            packet.WriteString(badge.Code);
        }

        packet.WriteInteger(equippedBadges.Count);
        foreach (var badge in equippedBadges)
        {
            packet.WriteInteger(badge.Slot);
            packet.WriteString(badge.Code);
        }
    }
}