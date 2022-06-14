using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class GetBuddyRequestsEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        ICollection<MessengerRequest> requests = session.GetHabbo().GetMessenger().Requests.Values.ToList();
        session.SendPacket(new BuddyRequestsComposer(requests));
        return Task.CompletedTask;
    }
}