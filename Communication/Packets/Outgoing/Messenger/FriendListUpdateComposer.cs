using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class FriendListUpdateComposer : ServerPacket
{
    public FriendListUpdateComposer(MessengerBuddy friend, BuddyModificationType modificationType)
        : base(ServerPacketHeader.FriendListUpdateMessageComposer)
    {
        WriteInteger(0);
        WriteInteger(1);
        WriteInteger((int)modificationType);
        if (modificationType == BuddyModificationType.Added || modificationType == BuddyModificationType.Updated)
            friend.Serialize(this);
        else
            WriteInteger(friend.Id);
    }

    public FriendListUpdateComposer(Dictionary<MessengerBuddy, BuddyModificationType> friends)
        : base(ServerPacketHeader.FriendListUpdateMessageComposer)
    {
        WriteInteger(0);
        WriteInteger(friends.Count);
        foreach (var (friend, modificationType) in friends)
        {
            WriteInteger((int)modificationType);
            if (modificationType == BuddyModificationType.Added || modificationType == BuddyModificationType.Updated)
                friend.Serialize(this);
            else
                WriteInteger(friend.Id);
        }
    }
}