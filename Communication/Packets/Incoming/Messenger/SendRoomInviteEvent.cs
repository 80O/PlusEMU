using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class SendRoomInviteEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly IDatabase _database;

    public SendRoomInviteEvent(IGameClientManager clientManager, IDatabase database)
    {
        _clientManager = clientManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().TimeMuted > 0)
        {
            session.SendNotification("Oops, you're currently muted - you cannot send room invitations.");
            return Task.CompletedTask;
        }
        var amount = packet.PopInt();
        if (amount > 500)
            return Task.CompletedTask; // don't send at all
        var targets = new List<int>();
        for (var i = 0; i < amount; i++)
        {
            var uid = packet.PopInt();
            if (i < 100) // limit to 100 people, keep looping until we fulfil the request though
                targets.Add(uid);
        }
        var message = StringCharFilter.Escape(packet.PopString());
        if (message.Length > 121)
            message = message.Substring(0, 121);
        foreach (var userId in targets)
        {
            if (!session.GetHabbo().GetMessenger().FriendshipExists(userId))
                continue;
            var client = _clientManager.GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null || client.GetHabbo().AllowMessengerInvites || client.GetHabbo().AllowConsoleMessages == false)
                continue;
            client.SendPacket(new RoomInviteComposer(session.GetHabbo().Id, message));
        }
        using var connection = _database.Connection();
        connection.Execute("INSERT INTO `chatlogs_console_invitations` (`user_id`,`message`,`timestamp`) VALUES (@userId, @message, UNIX_TIMESTAMP())",
            new { userId = session.GetHabbo().Id, message = message });
        return Task.CompletedTask;
    }
}