using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;

internal class GetYouTubePlaylistComposer : ServerPacket
{
    public GetYouTubePlaylistComposer(int itemId, ICollection<TelevisionItem> videos)
        : base(ServerPacketHeader.GetYouTubePlaylistMessageComposer)
    {
        WriteInteger(itemId);
        WriteInteger(videos.Count);
        foreach (var video in videos.ToList())
        {
            WriteString(video.YouTubeId);
            WriteString(video.Title); //Title
            WriteString(video.Description); //Description
        }
        WriteString("");
    }
}