using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class RequestBuddyEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;
    private readonly IMessengerDataLoader _messengerDataLoader;

    public RequestBuddyEvent(IQuestManager questManager, IMessengerDataLoader messengerDataLoader)
    {
        _questManager = questManager;
        _messengerDataLoader = messengerDataLoader;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var (userId, blocked) = await _messengerDataLoader.CanReceiveFriendRequests(packet.PopString());
        if (userId == 0 || blocked)
            return;

        session.GetHabbo().GetMessenger().SendFriendRequest(userId);
        _questManager.ProgressUserQuest(session, QuestType.SocialFriend);
        return;
    }
}