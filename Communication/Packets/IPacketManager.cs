using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets;

public interface IPacketManager
{
    void TryExecutePacket(GameClient session, ClientPacket packet);
}