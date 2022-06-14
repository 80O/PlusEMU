using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Users;

internal class OpenPlayerProfileEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IMessengerDataLoader _messengerDataLoader;

    public OpenPlayerProfileEvent(IGroupManager groupManager, IMessengerDataLoader messengerDataLoader)
    {
        _groupManager = groupManager;
        _messengerDataLoader = messengerDataLoader;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        packet.PopBoolean(); //IsMe?
        var targetData = PlusEnvironment.GetHabboById(userId);
        if (targetData == null)
        {
            session.SendNotification("An error occured whilst finding that user's profile.");
            return;
        }
        var groups = _groupManager.GetGroupsForUser(targetData.Id);


        var friendCount = await _messengerDataLoader.GetFriendCount(userId);
        session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
    }
}