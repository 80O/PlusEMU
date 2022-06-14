using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;
using Plus.Utilities.DependencyInjection;

namespace Plus.Communication.Packets;

[Transient]
public interface IPacketEvent
{
    Task Parse(GameClient session, ClientPacket packet);
}