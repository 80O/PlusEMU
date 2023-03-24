using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users;

public class HabboUserBadgesComposer : IServerPacket
{
    private readonly Habbo _habbo;
    public uint MessageId => ServerPacketHeader.HabboUserBadgesComposer;

    public HabboUserBadgesComposer(Habbo habbo)
    {
        _habbo = habbo;
    }

    public void Compose(IOutgoingPacket packet)
    {
        var equippedBadges = _habbo.Inventory.Badges.EquippedBadges.OrderBy(b => b.Slot).ToList();

        packet.WriteInteger(_habbo.Id);
        packet.WriteInteger(equippedBadges.Count);
        foreach (var badge in equippedBadges)
        {
            packet.WriteInteger(badge.Slot);
            packet.WriteString(badge.Code);
        }
    }
}