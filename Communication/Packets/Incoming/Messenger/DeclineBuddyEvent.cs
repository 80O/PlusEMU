using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class DeclineBuddyEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var declineAll = packet.PopBoolean();
        packet.PopInt(); //amount
        if (!declineAll)
        {
            var requestId = packet.PopInt();
            session.GetHabbo().GetMessenger().HandleRequest(requestId);
        }
        else
            session.GetHabbo().GetMessenger().HandleAllRequests();
        return Task.CompletedTask;
    }
}