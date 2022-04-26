using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class CloseIssueDefaultActionEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        return Task.CompletedTask;
    }
}