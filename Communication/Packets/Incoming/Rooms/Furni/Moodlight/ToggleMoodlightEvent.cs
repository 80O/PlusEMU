using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight;

internal class ToggleMoodlightEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;

    public ToggleMoodlightEvent(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true) || room.MoodlightData == null)
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(room.MoodlightData.ItemId);
        if (item == null || item.GetBaseItem().InteractionType != InteractionType.Moodlight)
            return Task.CompletedTask;
        if (room.MoodlightData.Enabled)
            room.MoodlightData.Disable();
        else
            room.MoodlightData.Enable();
        item.ExtraData = room.MoodlightData.GenerateExtraData();
        item.UpdateState();
        return Task.CompletedTask;
    }
}