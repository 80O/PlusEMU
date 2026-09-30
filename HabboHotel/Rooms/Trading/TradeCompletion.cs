namespace Plus.HabboHotel.Rooms.Trading;

/// <summary>Serializes cancellation and completion, including failed completion attempts.</summary>
public sealed class TradeCompletion
{
    public object SyncRoot { get; } = new();
    public bool Closed { get; private set; }

    public bool TryComplete(Action commit, Action apply, Action notify)
    {
        lock (SyncRoot)
        {
            if (Closed)
                return false;
            Closed = true;
            commit();
            apply();
            notify();
            return true;
        }
    }

    public bool TryCancel()
    {
        lock (SyncRoot)
        {
            if (Closed)
                return false;
            Closed = true;
            return true;
        }
    }
}
