using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class ScrGetUserInfoEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        session.SendPacket(new ScrSendUserInfoComposer());
        return Task.CompletedTask;
    }
}