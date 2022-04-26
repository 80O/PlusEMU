using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger;

internal class FriendListUpdateEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet) => Task.CompletedTask;
}