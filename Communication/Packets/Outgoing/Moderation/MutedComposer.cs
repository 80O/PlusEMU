namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class MutedComposer : ServerPacket
{
    public MutedComposer(double timeMuted)
        : base(ServerPacketHeader.MutedMessageComposer)
    {
        WriteInteger(Convert.ToInt32(timeMuted));
    }
}