using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class HabboSearchEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var query = StringCharFilter.Escape(packet.PopString().Replace("%", ""));
        if (query.Length < 1 || query.Length > 100)
            return Task.CompletedTask;
        var friends = new List<SearchResult>();
        var othersUsers = new List<SearchResult>();
        var results = SearchResultFactory.GetSearchResult(query);
        foreach (var result in results.ToList())
        {
            if (session.GetHabbo().GetMessenger().FriendshipExists(result.UserId))
                friends.Add(result);
            else
                othersUsers.Add(result);
        }
        session.SendPacket(new HabboSearchResultComposer(friends, othersUsers));
        return Task.CompletedTask;
    }
}