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
            session.GetHabbo().GetMessenger().DeclineFriendRequest(requestId);
        }
        else
        {
            foreach (var request in session.GetHabbo().GetMessenger().Requests.Values)
                session.GetHabbo().GetMessenger().DeclineFriendRequest(request.FromId);
        }
        return Task.CompletedTask;
    }
}