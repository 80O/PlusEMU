using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions;

internal class YouTubeVideoInformationEvent : IPacketEvent
{
    private readonly ITelevisionManager _televisionManager;

    public YouTubeVideoInformationEvent(ITelevisionManager televisionManager)
    {
        _televisionManager = televisionManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var itemId = packet.PopInt();
        var videoId = packet.PopString();
        foreach (var tele in _televisionManager.TelevisionList.ToList())
        {
            if (tele.YouTubeId != videoId)
                continue;
            session.SendPacket(new GetYouTubeVideoComposer(itemId, tele.YouTubeId));
        }
        return Task.CompletedTask;
    }
}