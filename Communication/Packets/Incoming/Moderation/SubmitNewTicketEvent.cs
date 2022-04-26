using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class SubmitNewTicketEvent : IPacketEvent
{
    public readonly IModerationManager _moderationManager;
    public readonly IGameClientManager _clientManager;
    public readonly IDatabase _database;

    public SubmitNewTicketEvent(IModerationManager moderationManager, IGameClientManager clientManager, IDatabase database)
    {
        _moderationManager = moderationManager;
        _clientManager = clientManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        // Run a quick check to see if we have any existing tickets.
        if (_moderationManager.UserHasTickets(session.GetHabbo().Id))
        {
            var pendingTicket = _moderationManager.GetTicketBySenderId(session.GetHabbo().Id);
            if (pendingTicket != null)
            {
                session.SendPacket(new CallForHelpPendingCallsComposer(pendingTicket));
                return Task.CompletedTask;
            }
        }
        var chats = new List<string>();
        var message = StringCharFilter.Escape(packet.PopString().Trim());
        var category = packet.PopInt();
        var reportedUserId = packet.PopInt();
        var type = packet.PopInt(); // Unsure on what this actually is.
        var reportedUser = PlusEnvironment.GetHabboById(reportedUserId);
        if (reportedUser == null)
        {
            // User doesn't exist.
            return Task.CompletedTask;
        }
        var messagecount = packet.PopInt();
        for (var i = 0; i < messagecount; i++)
        {
            packet.PopInt();
            chats.Add(packet.PopString());
        }
        var ticket = new ModerationTicket(1, type, category, UnixTimestamp.GetNow(), 1, session.GetHabbo(), reportedUser, message, session.GetHabbo().CurrentRoom, chats);
        if (!_moderationManager.TryAddTicket(ticket))
            return Task.CompletedTask;
        using (var dbClient = _database.GetQueryReactor())
        {
            // TODO: Come back to this.
            /*dbClient.SetQuery("INSERT INTO `moderation_tickets` (`score`,`type`,`status`,`sender_id`,`reported_id`,`moderator_id`,`message`,`room_id`,`room_name`,`timestamp`) VALUES (1, '" + Category + "', 'open', '" + Session.GetHabbo().Id + "', '" + ReportedUserId + "', '0', @message, '0', '', '" + PlusEnvironment.GetNow() + "')");
            dbClient.AddParameter("message", Message);
            dbClient.RunQuery();*/
            dbClient.RunQuery("UPDATE `user_info` SET `cfhs` = `cfhs` + '1' WHERE `user_id` = '" + session.GetHabbo().Id + "' LIMIT 1");
        }
        _clientManager.ModAlert("A new support ticket has been submitted!");
        _clientManager.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        return Task.CompletedTask;
    }
}