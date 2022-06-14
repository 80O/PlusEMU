using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class RemoveBuddyEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var amount = packet.PopInt();
        if (amount > 100)
            amount = 100;
        else if (amount < 0)
            return Task.CompletedTask;
        for (var i = 0; i < amount; i++)
        {
            var id = packet.PopInt();
            var friend = session.GetHabbo().GetMessenger().GetFriend(id);
            if (friend == null) continue;
            session.GetHabbo().GetMessenger().RemoveFriend(friend);
        }
        return Task.CompletedTask;
    }
}