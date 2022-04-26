using System;
using System.Linq;
using System.Threading.Tasks;
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
        habbo.Relationships = habbo.Relationships.OrderBy(x => Random.Shared.Next()).ToDictionary(item => item.Key, item => item.Value);
        session.SendPacket(new GetRelationshipsComposer(habbo));
        return Task.CompletedTask;
    }
}