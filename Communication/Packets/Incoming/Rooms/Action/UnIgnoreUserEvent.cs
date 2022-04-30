using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Ignores;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class UnIgnoreUserEvent : IPacketEvent
{
    private readonly IIgnoresManager _ignoresManager;

    public UnIgnoreUserEvent(IIgnoresManager ignoresManager)
    {
        _ignoresManager = ignoresManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var username = packet.PopString();
        var player = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo();
        if (player == null)
            return Task.CompletedTask;
        if (!session.GetHabbo().GetIgnores().TryGet(player.Id))
            return Task.CompletedTask;
        if (session.GetHabbo().GetIgnores().TryRemove(player.Id))
        {
            _ignoresManager.UnIgnoreUser(session.GetHabbo().Id, player.Id);
            session.SendPacket(new IgnoreStatusComposer(3, player.Username));

        }
        return Task.CompletedTask;
    }
}