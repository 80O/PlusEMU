using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Ignores;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class IgnoreUserEvent : IPacketEvent
{
    IIgnoresComponent _ignoresComponent;
    IAchievementManager _achievementManager;

    public IgnoreUserEvent(IAchievementManager achievementManager, IIgnoresComponent ignoresComponent)
    {
        _achievementManager = achievementManager;
        _ignoresComponent = ignoresComponent;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var username = packet.PopString();

        var ignoredid = await _ignoresComponent.IgnoreUser(session.GetHabbo(), PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo());
        if(ignoredid != null) 
            session.SendPacket(new IgnoreStatusComposer(1, ignoredid.Username));
        _achievementManager.ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
    }
}