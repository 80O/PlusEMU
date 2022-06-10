using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class SetChatStylePreferenceEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var chatBubbleId = packet.PopInt();

        session.GetHabbo().CustomBubbleId = chatBubbleId;
        session.GetHabbo().SaveChatBubble(chatBubbleId.ToString());

        return Task.CompletedTask;
    }
}