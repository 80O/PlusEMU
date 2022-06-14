namespace Plus.Communication.Packets.Outgoing.Sound;

internal class SoundSettingsComposer : ServerPacket
{
    public SoundSettingsComposer(IEnumerable<int> volumes, bool chatPreference, bool invitesStatus, bool focusPreference, int friendBarState)
        : base(ServerPacketHeader.SoundSettingsMessageComposer)
    {
        foreach (var volume in volumes) WriteInteger(volume);
        WriteBoolean(chatPreference);
        WriteBoolean(invitesStatus);
        WriteBoolean(focusPreference);
        WriteInteger(friendBarState);
        WriteInteger(0);
        WriteInteger(0);
    }
}