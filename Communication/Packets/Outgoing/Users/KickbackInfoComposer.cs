using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Users;

public sealed class KickbackInfoComposer : IServerPacket
{
    public uint MessageId => ServerPacketHeader.KickbackInfoComposer;

    public void Compose(IOutgoingPacket packet)
    {
        // The emulator does not track HC kickback history yet. Send the complete
        // protocol payload so the client can open the HC center without null data.
        packet.WriteInteger(0); // current HC streak
        packet.WriteString(string.Empty); // first subscription date
        packet.WriteDouble(0); // kickback percentage
        packet.WriteInteger(0); // total credits missed
        packet.WriteInteger(0); // total credits rewarded
        packet.WriteInteger(0); // total credits spent
        packet.WriteInteger(0); // streak bonus reward
        packet.WriteInteger(0); // monthly spend reward
        packet.WriteInteger(0); // seconds until payday
    }
}
