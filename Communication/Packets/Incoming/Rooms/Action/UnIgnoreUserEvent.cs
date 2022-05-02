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
        var username = packet.PopString();

        _ignoresComponent.UnIgnoreUser(session.GetHabbo(), PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo());
        session.SendPacket(new IgnoreStatusComposer(3, PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username).GetHabbo().Username));

        return Task.CompletedTask;
    }
}