using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetSelectedBadgesEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        var habbo = PlusEnvironment.GetHabboById(userId);
        if (habbo == null)
            return Task.CompletedTask;
        session.SendPacket(new HabboUserBadgesComposer(habbo));
        return Task.CompletedTask;
    }
}