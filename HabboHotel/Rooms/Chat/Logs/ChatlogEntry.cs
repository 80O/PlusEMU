using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Logs;

public sealed class ChatlogEntry
{
    private readonly WeakReference _playerReference;
    private readonly WeakReference _roomReference;

    public ChatlogEntry(int playerId, int roomId, string message, double timestamp, Habbo player = null, RoomData instance = null)
    {
        PlayerId = playerId;
        RoomId = roomId;
        Message = message;
        Timestamp = timestamp;
        if (player != null)
            _playerReference = new WeakReference(player);
        if (instance != null)
            _roomReference = new WeakReference(instance);
    }

    public int PlayerId { get; }

    public int RoomId { get; }

    public string Message { get; }

    public double Timestamp { get; }

    public Habbo PlayerNullable()
    {
        if (_playerReference.IsAlive)
        {
            var playerObj = (Habbo)_playerReference.Target;
            return playerObj;
        }
        return null;
    }

    public Room RoomNullable()
    {
        if (_roomReference.IsAlive)
        {
            var roomObj = (Room)_roomReference.Target;
            if (roomObj.MDisposed)
                return null;
            return roomObj;
        }
        return null;
    }
}