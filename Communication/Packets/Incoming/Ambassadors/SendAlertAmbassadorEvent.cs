using Plus.HabboHotel.GameClients;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Ambassadors;

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
