using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class MessengerInitEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.GetHabbo().GetMessenger().OnStatusChanged(false);
        ICollection<MessengerBuddy> friends = new List<MessengerBuddy>();
        foreach (var buddy in session.GetHabbo().GetMessenger().GetFriends().ToList())
        {
            if (buddy == null || buddy.IsOnline)
                continue;
            friends.Add(buddy);
        }
        session.SendPacket(new MessengerInitComposer());
        var page = 0;
        if (!friends.Any())
            session.SendPacket(new BuddyListComposer(friends, session.GetHabbo(), 1, 0));
        else
        {
            var pages = (friends.Count() - 1) / 500 + 1;
            foreach (ICollection<MessengerBuddy> batch in friends.Chunk(500))
            {
                session.SendPacket(new BuddyListComposer(batch.ToList(), session.GetHabbo(), pages, page));
                page++;
            }
        }
        session.GetHabbo().GetMessenger().ProcessOfflineMessages();
        return Task.CompletedTask;
    }
}