using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Ignores;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class UnIgnoreUserEvent : IPacketEvent
{
    private readonly IIgnoresComponent _ignoresComponent;

    public UnIgnoreUserEvent(IIgnoresComponent ignoresComponent)
    {
        _ignoresComponent = ignoresComponent;
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
        if (!session.GetHabbo().GetIgnores().TryGet(player))
            return Task.CompletedTask;
        if (session.GetHabbo().GetIgnores().TryRemove(player))
        {
            _ignoresComponent.UnIgnoreUser(session.GetHabbo(), player);
            session.SendPacket(new IgnoreStatusComposer(3, player.Username));
        }
        return Task.CompletedTask;
    }
}