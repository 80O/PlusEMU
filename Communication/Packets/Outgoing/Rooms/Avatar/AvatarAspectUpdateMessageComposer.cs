namespace Plus.Communication.Packets.Outgoing.Rooms.Avatar;

internal class AvatarAspectUpdateMessageComposer : ServerPacket
{
    public AvatarAspectUpdateMessageComposer(string figure, string gender)
        : base(ServerPacketHeader.AvatarAspectUpdateMessageComposer)
    {
        WriteString(figure);
        WriteString(gender);
    }
}