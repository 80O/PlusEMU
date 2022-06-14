using Plus.Communication.Attributes;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class UniqueIdEvent : IPacketEvent
{
    private readonly IModerationManager _moderationManager;

    public UniqueIdEvent(IModerationManager moderationManager)
    {
        _moderationManager = moderationManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        packet.PopString();
        var machineId = packet.PopString();
        session.MachineId = machineId;
        if (_moderationManager.HasMachineBanCheck(machineId))
        {
            session.Disconnect();
            return Task.CompletedTask;
        }
        session.SendPacket(new SetUniqueIdComposer(machineId));
        return Task.CompletedTask;
    }
}