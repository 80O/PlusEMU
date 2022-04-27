using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class HabboSearchResultComposer : ServerPacket
{
    public HabboSearchResultComposer(List<SearchResult> friends, List<SearchResult> otherUsers)
        : base(ServerPacketHeader.HabboSearchResultMessageComposer)
    {
        WriteInteger(friends.Count);
        foreach (var friend in friends.ToList())
        {
            var online = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friend.UserId) != null;
            WriteInteger(friend.UserId);
            WriteString(friend.Username);
            WriteString(friend.Motto);
            WriteBoolean(online);
            WriteBoolean(false);
            WriteString(string.Empty);
            WriteInteger(0);
            WriteString(friend.Figure);
            WriteString(friend.LastOnline);
        }
        WriteInteger(otherUsers.Count);
        foreach (var otherUser in otherUsers.ToList())
        {
            var online = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(otherUser.UserId) != null;
            WriteInteger(otherUser.UserId);
            WriteString(otherUser.Username);
            WriteString(otherUser.Motto);
            WriteBoolean(online);
            WriteBoolean(false);
            WriteString(string.Empty);
            WriteInteger(0);
            WriteString(otherUser.Figure);
            WriteString(otherUser.LastOnline);
        }
    }
}