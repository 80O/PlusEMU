using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms;

public interface IRoomManager
{
    int Count { get; }
    void OnCycle();
    void LoadModels();
    bool LoadModel(string id);
    void ReloadModel(string id);
    bool TryGetModel(string id, out RoomModel model);
    void UnloadRoom(int roomId);
    bool TryLoadRoom(int roomId, out Room room);
    List<Room> SearchGroupRooms(string query);
    List<Room> SearchTaggedRooms(string query);
    List<Room> GetPopularRooms(int category, int amount = 50);
    List<Room> GetRecommendedRooms(int amount = 50, int currentRoomId = 0);
    List<Room> GetPopularRatedRooms(int amount = 50);
    List<Room> GetRoomsByCategory(int category, int amount = 50);
    List<Room> GetOnGoingRoomPromotions(int mode, int amount = 50);
    List<Room> GetPromotedRooms(int categoryId, int amount = 50);
    List<Room> GetGroupRooms(int amount = 50);
    List<Room> GetRoomsByIds(List<int> ids, int amount = 50);
    Room TryGetRandomLoadedRoom();
    bool TryGetRoom(int roomId, out Room room);

    RoomData CreateRoom(GameClient session, string name, string description, int category, int maxVisitors, int tradeSettings, RoomModel model, string wallpaper = "0.0", string floor = "0.0",
        string landscape = "0.0", int wallthick = 0, int floorthick = 0);

    ICollection<Room> GetRooms();
    void Dispose();
}