using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Profile;

namespace Plus.Communication.Packets.Incoming.Users;

internal class OpenPlayerProfileEvent : IPacketEvent
{
    private readonly IProfileManager _profile;

    public OpenPlayerProfileEvent(IProfileManager profile) => _profile = profile;

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        packet.PopBoolean(); //IsMe?
        var targetData = _profile.GetProfile(PlusEnvironment.GetHabboById(userId));
        var groups = _profile.GetGroups(PlusEnvironment.GetHabboById(userId));
        var friendCount = _profile.GetFriendCount(userId);
        if(targetData != null)
            session.SendPacket(new ProfileInformationComposer(targetData, session, groups,await friendCount));
    }
}