using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class BuddyListComposer : ServerPacket
{
    public BuddyListComposer(ICollection<MessengerBuddy> friends, Habbo player, int pages, int page)
        : base(ServerPacketHeader.BuddyListMessageComposer)
    {
        WriteInteger(pages); // Pages
        WriteInteger(page); // Page
        WriteInteger(friends.Count);
        foreach (var friend in friends.ToList())
            friend.Serialize(this);
    }
}