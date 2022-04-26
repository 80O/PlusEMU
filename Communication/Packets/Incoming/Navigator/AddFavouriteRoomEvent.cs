using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator;

public class AddFavouriteRoomEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public AddFavouriteRoomEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var roomId = packet.PopInt();
        if (!RoomFactory.TryGetData(roomId, out var data))
            return Task.CompletedTask;
        if (data == null || session.GetHabbo().FavoriteRooms.Count >= 30 || session.GetHabbo().FavoriteRooms.Contains(roomId))
        {
            // send packet that favourites is full.
            return Task.CompletedTask;
        }
        session.GetHabbo().FavoriteRooms.Add(roomId);
        session.SendPacket(new UpdateFavouriteRoomComposer(roomId, true));
        using var dbClient = _database.GetQueryReactor();
        dbClient.RunQuery("INSERT INTO user_favorites (user_id,room_id) VALUES (" + session.GetHabbo().Id + "," + roomId + ")");
        return Task.CompletedTask;
    }
}