using Plus.HabboHotel.Ambassadors;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Ambassadors
{
    internal class AmbassadorSendAlertEvent : IPacketEvent
    {
        private readonly IAmbassadorsManager _ambassadorsManager;

        public AmbassadorSendAlertEvent(IAmbassadorsManager ambassadorsManager) => _ambassadorsManager = ambassadorsManager;

        public async Task Parse(GameClient session, ClientPacket packet)
        {

            var userid = packet.PopInt();
            var target = PlusEnvironment.GetHabboById(userid);

            await _ambassadorsManager.Warn(session.GetHabbo(), target, "Alert");

        }
    }
}
