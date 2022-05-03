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

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var username = packet.PopString();

        var ignoredid = await _ignoresComponent.UnIgnoreUser(session.GetHabbo(), PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo());
        if (ignoredid != null)
            session.SendPacket(new IgnoreStatusComposer(3, ignoredid.Username));
    }
}