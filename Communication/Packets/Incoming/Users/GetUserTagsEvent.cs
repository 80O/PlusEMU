using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetUserTagsEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        session.SendPacket(new UserTagsComposer(userId));
        return Task.CompletedTask;
    }
}