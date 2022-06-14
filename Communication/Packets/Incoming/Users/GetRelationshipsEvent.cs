using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetRelationshipsEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var habbo = PlusEnvironment.GetHabboById(packet.PopInt());
        if (habbo == null)
            return Task.CompletedTask;
        session.SendPacket(new GetRelationshipsComposer(habbo));
        return Task.CompletedTask;
    }
}