using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class AcceptBuddyEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var amount = packet.PopInt();
        if (amount > 50)
            amount = 50;
        else if (amount < 0)
            return Task.CompletedTask;
        for (var i = 0; i < amount; i++)
        {
            var requestId = packet.PopInt();
            if (!session.GetHabbo().GetMessenger().TryGetRequest(requestId, out var request))
                continue;
            if (request.To != session.GetHabbo().Id)
                return Task.CompletedTask;
            if (!session.GetHabbo().GetMessenger().FriendshipExists(request.To))
                session.GetHabbo().GetMessenger().CreateFriendship(request.From);
            session.GetHabbo().GetMessenger().HandleRequest(requestId);
        }
        return Task.CompletedTask;
    }
}