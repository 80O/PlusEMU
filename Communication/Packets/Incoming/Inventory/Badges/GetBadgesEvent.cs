using Plus.Communication.Packets.Outgoing.Inventory.Badges;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Badges;

internal class GetBadgesEvent : IPacketEvent
{
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        session.Send(new BadgesComposer(session.GetHabbo().Id, session.GetHabbo().Inventory.Badges.Badges));
        return Task.CompletedTask;
    }
}
