using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class PickTicketEvent : IPacketEvent
{
    public readonly IModerationManager _moderationManager;
    public readonly IGameClientManager _clientManager;

    public PickTicketEvent(IModerationManager moderationManager, IGameClientManager clientManager)
    {
        _moderationManager = moderationManager;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        packet.PopInt(); //Junk
        var ticketId = packet.PopInt();
        if (!_moderationManager.TryGetTicket(ticketId, out var ticket))
            return Task.CompletedTask;
        ticket.Moderator = session.GetHabbo();
        _clientManager.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        return Task.CompletedTask;
    }
}