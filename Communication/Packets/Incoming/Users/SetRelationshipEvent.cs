using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database;
using Plus.HabboHotel.Friends;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class SetRelationshipEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;
    private readonly IMessengerDataLoader _messengerDataLoader;

    public SetRelationshipEvent(IGameClientManager clientManager, IDatabase database, IMessengerDataLoader messengerDataLoader)
    {
        _clientManager = clientManager;
        _database = database;
        _messengerDataLoader = messengerDataLoader;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var user = packet.PopInt();
        var type = packet.PopInt();
        var friend = session.GetHabbo().GetMessenger().GetFriend(user);
        if (friend == null)
        {
            session.SendPacket(new BroadcastMessageAlertComposer("Oops, you can only set a relationship where a friendship exists."));
            return;
        }
        if (type < 0 || type > 3)
        {
            session.SendPacket(new BroadcastMessageAlertComposer("Oops, you've chosen an invalid relationship type."));
            return;
        }

        friend.Relationship = type;
        await _messengerDataLoader.SetRelationship(session.GetHabbo().Id, friend.Id, friend.Relationship);
        session.GetHabbo().GetMessenger().UpdateFriend(friend);
        return;
    }
}