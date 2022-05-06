using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Profile;

namespace Plus.Communication.Packets.Incoming.Users;

internal class OpenPlayerProfileEvent : IPacketEvent
{
    private readonly IProfile _profile;

    public OpenPlayerProfileEvent(IProfile profile)
    {
        _profile = profile;
    }

    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        packet.PopBoolean(); //IsMe?
        var targetData = await _profile.GetProfile(PlusEnvironment.GetHabboById(userId));
        var groups = await _profile.GetGroups(PlusEnvironment.GetHabboById(userId));
        var friendCount = await _profile.GetFriendCount(PlusEnvironment.GetHabboById(userId));
        if(targetData != null)
            session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
    }
}