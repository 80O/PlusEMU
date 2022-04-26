using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger.FriendBar;

namespace Plus.Communication.Packets.Incoming.Misc;

internal class SetFriendBarStateEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.GetHabbo().FriendbarState = FriendBarStateUtility.GetEnum(packet.PopInt());
        session.SendPacket(new SoundSettingsComposer(session.GetHabbo().ClientVolume, session.GetHabbo().ChatPreference, session.GetHabbo().AllowMessengerInvites, session.GetHabbo().FocusPreference,
            FriendBarStateUtility.GetInt(session.GetHabbo().FriendbarState)));
        return Task.CompletedTask;
    }
}