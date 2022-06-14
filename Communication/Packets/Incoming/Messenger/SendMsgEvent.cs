using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class SendMsgEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;

    public SendMsgEvent(IWordFilterManager wordFilterManager)
    {
        _wordFilterManager = wordFilterManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        var friend = session.GetHabbo().GetMessenger().GetFriend(userId);
        if (friend == null)
            session.SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.NotFriends, userId));
        var message = _wordFilterManager.CheckMessage(packet.PopString());
        if (string.IsNullOrWhiteSpace(message))
            return Task.CompletedTask;
        if (session.GetHabbo().TimeMuted > 0)
        {
            session.SendNotification("Oops, you're currently muted - you cannot send messages.");
            return Task.CompletedTask;
        }

        var error = session.GetHabbo().GetMessenger().SendMessage(friend, message);
        if (error == MessageError.Flooding)
            session.SendNotification("You cannot send a message, you have flooded the console.\n\nYou can send a message in 60 seconds.");

        return Task.CompletedTask;
    }
}