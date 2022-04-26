using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Help;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Help;

internal class SendBullyReportEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new SendBullyReportComposer());
        return Task.CompletedTask;
    }
}