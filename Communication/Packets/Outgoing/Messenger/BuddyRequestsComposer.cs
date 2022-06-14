using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class BuddyRequestsComposer : ServerPacket
{
    public BuddyRequestsComposer(ICollection<MessengerRequest> requests)
        : base(ServerPacketHeader.BuddyRequestsMessageComposer)
    {
        WriteInteger(requests.Count);
        WriteInteger(requests.Count);
        foreach (var request in requests)
        {
            WriteInteger(request.FromId);
            WriteString(request.Username);
            var user = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(request.FromId);
            WriteString(user != null ? user.Look : "");
        }
    }
}