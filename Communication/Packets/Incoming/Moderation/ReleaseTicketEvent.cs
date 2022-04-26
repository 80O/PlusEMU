using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ReleaseTicketEvent : IPacketEvent
{
    public readonly IModerationManager _moderationManager;
    public readonly IGameClientManager _clientManager;

    public ReleaseTicketEvent(IModerationManager moderationManager, IGameClientManager clientManager)
    {
        _moderationManager = moderationManager;
        _clientManager = clientManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        var amount = packet.PopInt();
        for (var i = 0; i < amount; i++)
        {
            if (!_moderationManager.TryGetTicket(packet.PopInt(), out var ticket))
                continue;
            ticket.Moderator = null;
            _clientManager.SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
        return Task.CompletedTask;
    }
}