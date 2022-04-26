using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class SetTonerEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IDatabase _database;
    public SetTonerEvent(IRoomManager roomManager, IDatabase database)
    {
        _roomManager = roomManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        if (!room.CheckRights(session, true))
            return Task.CompletedTask;
        if (room.TonerData == null)
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(room.TonerData.ItemId);
        if (item == null || item.GetBaseItem().InteractionType != InteractionType.Toner)
            return Task.CompletedTask;
        packet.PopInt(); //id
        var int1 = packet.PopInt();
        var int2 = packet.PopInt();
        var int3 = packet.PopInt();
        if (int1 > 255 || int2 > 255 || int3 > 255)
            return Task.CompletedTask;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `room_items_toner` SET `enabled` = '1', `data1` = @data1, `data2` = @data2, `data3` = @data3 WHERE `id` = @itemId LIMIT 1");
            dbClient.AddParameter("itemId", item.Id);
            dbClient.AddParameter("data1", int1);
            dbClient.AddParameter("data3", int3);
            dbClient.AddParameter("data2", int2);
            dbClient.RunQuery();
        }
        room.TonerData.Hue = int1;
        room.TonerData.Saturation = int2;
        room.TonerData.Lightness = int3;
        room.TonerData.Enabled = 1;
        room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
        item.UpdateState();
        return Task.CompletedTask;
    }
}