using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.Badges;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.Badges;

internal class GetBadgesEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new BadgesComposer(session));
        return Task.CompletedTask;
    }
}