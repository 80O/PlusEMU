using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Help;

internal class GetSanctionStatusEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        //Session.SendMessage(new SanctionStatusComposer());
        return Task.CompletedTask;
    }
}