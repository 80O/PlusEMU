namespace Plus.HabboHotel.Rooms.Chat.Logs;

public sealed class ChatlogManager : IChatlogManager
{
    private const int FlushOnCount = 10;

    private readonly List<ChatlogEntry> _chatlogs;
    private readonly ReaderWriterLockSlim _lock;

    public ChatlogManager()
    {
        _chatlogs = new List<ChatlogEntry>();
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public void StoreChatlog(ChatlogEntry entry)
    {
        _lock.EnterUpgradeableReadLock();
        _chatlogs.Add(entry);
        OnChatlogStore();
        _lock.ExitUpgradeableReadLock();
    }

    private void OnChatlogStore()
    {
        if (_chatlogs.Count >= FlushOnCount)
            FlushAndSave();
    }

    public void FlushAndSave()
    {
        _lock.EnterWriteLock();
        if (_chatlogs.Count > 0)
        {
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            foreach (var entry in _chatlogs)
            {
                dbClient.SetQuery("INSERT INTO chatlogs (`user_id`, `room_id`, `timestamp`, `message`) VALUES " + "(@uid, @rid, @time, @msg)");
                dbClient.AddParameter("uid", entry.PlayerId);
                dbClient.AddParameter("rid", entry.RoomId);
                dbClient.AddParameter("time", entry.Timestamp);
                dbClient.AddParameter("msg", entry.Message);
                dbClient.RunQuery();
            }
        }
        _chatlogs.Clear();
        _lock.ExitWriteLock();
    }
}