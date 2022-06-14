namespace Plus.HabboHotel.Users.Ignores;

public sealed class IgnoresComponent
{
    private readonly List<int> _ignoredUsers;
    public IReadOnlyCollection<int> IgnoredUsers => _ignoredUsers;

    public event EventHandler<IgnoreStatusUpdatedEventArgs>? UserIgnored;
    public event EventHandler<IgnoreStatusUpdatedEventArgs>? UserUnignored;

    public IgnoresComponent(List<int> ignoredUsers)
    {
        _ignoredUsers = ignoredUsers;
    }

    public bool TryAdd(int userId)
    {
        if (_ignoredUsers.Contains(userId))
            return false;
        _ignoredUsers.Add(userId);
        return true;
    }

    public bool IsIgnored(int userId) => _ignoredUsers.Contains(userId);

    public bool Ignore(int userId)
    {
        if (_ignoredUsers.Contains(userId)) return false;
        _ignoredUsers.Add(userId);
        UserIgnored?.Invoke(this, new(userId));
        return true;
    }

    public bool Unignore(int userId)
    {
        if (!_ignoredUsers.Remove(userId)) return false;
        UserUnignored?.Invoke(this, new(userId));
        return true;
    }
}