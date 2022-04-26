using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions;

internal class GetYouTubeTelevisionEvent : IPacketEvent
{
    private readonly ITelevisionManager _televisionManager;

    public GetYouTubeTelevisionEvent(ITelevisionManager televisionManager)
    {
        _televisionManager = televisionManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var videos = _televisionManager.TelevisionList;
        if (videos.Count == 0)
        {
            session.SendNotification("Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
            return Task.CompletedTask;
        }
        var dict = _televisionManager.Televisions;
        foreach (var value in RandomValues(dict).Take(1)) session.SendPacket(new GetYouTubeVideoComposer(itemId, value.YouTubeId));
        session.SendPacket(new GetYouTubePlaylistComposer(itemId, videos));
        return Task.CompletedTask;
    }

    private static IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        var values = dict.Values.ToList();
        var size = dict.Count;
        while (true) yield return values[Random.Shared.Next(size)];
    }
}