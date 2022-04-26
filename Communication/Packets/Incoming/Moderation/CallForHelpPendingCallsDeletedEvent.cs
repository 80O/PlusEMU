using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class CallForHelpPendingCallsDeletedEvent : IPacketEvent
{
    private readonly IModerationManager _moderationManager;
    private readonly IGameClientManager _clientManager;

    public CallForHelpPendingCallsDeletedEvent(IModerationManager moderationManager, IGameClientManager clientManager)
    {
        _moderationManager = moderationManager;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (_moderationManager.UserHasTickets(session.GetHabbo().Id))
        {
            var pendingTicket = _moderationManager.GetTicketBySenderId(session.GetHabbo().Id);
            if (pendingTicket != null)
            {
                pendingTicket.Answered = true;
                _clientManager.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, pendingTicket), "mod_tool");
            }
        }
        return Task.CompletedTask;
    }
}