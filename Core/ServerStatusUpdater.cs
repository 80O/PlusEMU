using NLog;

namespace Plus.Core;

public class ServerStatusUpdater : IDisposable, IServerStatusUpdater
{
    private const int UpdateInSeconds = 30;
    private static readonly ILogger _log = LogManager.GetLogger("Plus.Core.ServerStatusUpdater");

    private Timer _timer;

    public void Dispose()
    {
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
        }
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Init()
    {
        _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(UpdateInSeconds), TimeSpan.FromSeconds(UpdateInSeconds));
        Console.Title = "Plus Emulator - 0 users online - 0 rooms loaded - 0 day(s) 0 hour(s) uptime";
        _log.Info("Server Status Updater has been started.");
    }

    public void OnTick(object obj)
    {
        UpdateOnlineUsers();
    }

    private void UpdateOnlineUsers()
    {
        var uptime = DateTime.Now - PlusEnvironment.ServerStarted;
        var usersOnline = PlusEnvironment.GetGame().GetClientManager().Count;
        var roomCount = PlusEnvironment.GetGame().GetRoomManager().Count;
        Console.Title = "Plus Emulator - " + usersOnline + " users online - " + roomCount + " rooms loaded - " + uptime.Days + " day(s) " + uptime.Hours + " hour(s) uptime";
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @users, `loaded_rooms` = @loadedRooms LIMIT 1;");
        dbClient.AddParameter("users", usersOnline);
        dbClient.AddParameter("loadedRooms", roomCount);
        dbClient.RunQuery();
    }
}