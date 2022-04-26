using System.Threading.Tasks;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets;

public interface IPacketEvent
{
    Task Parse(GameClient session, ClientPacket packet);
}