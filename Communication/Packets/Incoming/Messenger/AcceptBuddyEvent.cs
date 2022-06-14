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

        var messenger = session.GetHabbo().GetMessenger();

        for (var i = 0; i < amount; i++)
        {
            var requestId = packet.PopInt();
            messenger.AcceptFriendRequest(requestId);
        }
        return Task.CompletedTask;
    }
}