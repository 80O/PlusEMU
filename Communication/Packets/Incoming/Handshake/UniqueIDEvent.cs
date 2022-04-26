using System.Threading.Tasks;
using Plus.Communication.Attributes;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class UniqueIdEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        packet.PopString();
        var machineId = packet.PopString();
        session.MachineId = machineId;
        session.SendPacket(new SetUniqueIdComposer(machineId));
        return Task.CompletedTask;
    }
}