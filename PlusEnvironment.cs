using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NLog;
using Plus.Communication.ConnectionManager;
using Plus.Communication.Encryption;
using Plus.Communication.Encryption.Keys;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Rcon;
using Plus.Core;
using Plus.Core.FigureData;
using Plus.Core.Language;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.UserData;
using Plus.Utilities;

namespace Plus;

public class PlusEnvironment : IPlusEnvironment
{
    public const string PrettyVersion = "Plus Emulator";
    public const string PrettyBuild = "3.4.3.0";
    private static readonly ILogger Log = LogManager.GetLogger("Plus.PlusEnvironment");

    private static Encoding _defaultEncoding;
    public static CultureInfo CultureInfo;

    private static IGame _game;
    private static ConfigurationData _configuration;
    private static IConnectionHandling _connectionManager;
    private static ILanguageManager _languageManager;
    private static ISettingsManager _settingsManager;
    private static IDatabase _database;
    private static IRconSocket _rcon;
    private static IFigureDataManager _figureManager;

    // TODO: Get rid?
    public static bool Event = false;
    public static DateTime LastEvent;
    public static DateTime ServerStarted;

    private static readonly List<char> Allowedchars = new(new[]
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
        'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
        'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
    });

    private static readonly ConcurrentDictionary<int, Habbo> _usersCached = new();

    public static string SwfRevision = "";
    private readonly IEnumerable<IStartable> _startableTasks;

    public PlusEnvironment(ConfigurationData configurationData,
        IDatabase database,
        ILanguageManager languageManager,
        ISettingsManager settingsManager,
        IFigureDataManager figureDataManager,
        IGame game,
        IEnumerable<IStartable> startableTasks,
        IConnectionHandling connectionHandling,
        IRconSocket rconSocket)
    {
        _database = database;
        _configuration = configurationData;
        _languageManager = languageManager;
        _settingsManager = settingsManager;
        _figureManager = figureDataManager;
        _game = game;
        _startableTasks = startableTasks;
        _connectionManager = connectionHandling;
        _rcon = rconSocket;
    }

    public async Task<bool> Start()
    {
        ServerStarted = DateTime.Now;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine();
        Console.WriteLine("                     ____  __           ________  _____  __");
        Console.WriteLine(@"                    / __ \/ /_  _______/ ____/  |/  / / / /");
        Console.WriteLine("                   / /_/ / / / / / ___/ __/ / /|_/ / / / / ");
        Console.WriteLine("                  / ____/ / /_/ (__  ) /___/ /  / / /_/ /  ");
        Console.WriteLine(@"                 /_/   /_/\__,_/____/_____/_/  /_/\____/ ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("                                " + PrettyVersion + " <Build " + PrettyBuild + ">");
        Console.WriteLine("                                http://PlusIndustry.com");
        Console.WriteLine("");
        Console.Title = "Loading Plus Emulator";
        _defaultEncoding = Encoding.Default;
        Console.WriteLine("");
        Console.WriteLine("");
        CultureInfo = CultureInfo.InvariantCulture;
        try
        {
            if (!_database.IsConnected())
            {
                Log.Error("Failed to Connect to the specified MySQL server.");
                Console.ReadKey(true);
                return false;
            }
            Log.Info("Connected to Database!");

            //Reset our statistics first.
            await ResetStatistics();

            //Get the configuration & Game set.
            await _languageManager.Reload();
            await _settingsManager.Reload();
            _figureManager.Init();

            //Have our encryption ready.
            HabboEncryptionV2.Initialize(new RsaKeys());

            //Make sure Rcon is connected before we allow clients to Connect.
            _rcon.Init(GetConfig().Data["rcon.tcp.bindip"], int.Parse(GetConfig().Data["rcon.tcp.port"]), GetConfig().Data["rcon.tcp.allowedaddr"].Split(Convert.ToChar(";")));

            //Accept connections.
            _connectionManager.Init(int.Parse(GetConfig().Data["game.tcp.port"]), int.Parse(GetConfig().Data["game.tcp.conlimit"]),
                int.Parse(GetConfig().Data["game.tcp.conperip"]), GetConfig().Data["game.tcp.enablenagles"].ToLower() == "true");

            // Allow services to self initialize
            foreach (var task in _startableTasks)
                await task.Start();

            await _game.Init();
            _game.StartGameLoop();
            var timeUsed = DateTime.Now - ServerStarted;
            Console.WriteLine();
            Log.Info("EMULATOR -> READY! (" + timeUsed.Seconds + " s, " + timeUsed.Milliseconds + " ms)");
        }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
        catch (KeyNotFoundException e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
        {
            Log.Error("Please check your configuration file - some values appear to be missing.");
            Log.Error("Press any key to shut down ...");
            Console.ReadKey(true);
            return false;
        }
        catch (InvalidOperationException e)
        {
            Log.Error("Failed to initialize PlusEmulator: " + e.Message);
            Log.Error("Press any key to shut down ...");
            Console.ReadKey(true);
            return false;
        }
        catch (Exception e)
        {
            Log.Error("Fatal error during startup: " + e);
            Log.Error("Press a key to exit");
            Console.ReadKey();
            return false;
        }

        return true;
    }

    private async Task ResetStatistics()
    {
        using var connection = _database.Connection();
        await connection.ExecuteAsync("TRUNCATE `catalog_marketplace_data`");
        await connection.ExecuteAsync("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0';");
        await connection.ExecuteAsync("UPDATE `users` SET `online` = '0' WHERE `online` = '1'");
        await connection.ExecuteAsync("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
    }

    public static bool EnumToBool(string @enum) => @enum == "1";

    public static string BoolToEnum(bool @bool) => @bool ? "1" : "0";

    public static double GetUnixTimestamp()
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return ts.TotalSeconds;
    }

    public static long Now()
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        var unixTime = ts.TotalMilliseconds;
        return (long)unixTime;
    }

    public static string FilterFigure(string figure)
    {
        foreach (var character in figure)
        {
            if (!IsValid(character))
                return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
        }
        return figure;
    }

    private static bool IsValid(char character) => Allowedchars.Contains(character);

    public static bool IsValidAlphaNumeric(string inputStr)
    {
        inputStr = inputStr.ToLower();
        if (string.IsNullOrEmpty(inputStr)) return false;
        for (var i = 0; i < inputStr.Length; i++)
        {
            if (!IsValid(inputStr[i]))
                return false;
        }
        return true;
    }

    public static string GetUsernameById(int userId)
    {
        var name = "Unknown User";
        var client = GetGame().GetClientManager().GetClientByUserId(userId);
        if (client != null && client.GetHabbo() != null)
            return client.GetHabbo().Username;
        var user = GetGame().GetCacheManager().GenerateUser(userId);
        if (user != null)
            return user.Username;
        using (var dbClient = GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @id LIMIT 1");
            dbClient.AddParameter("id", userId);
            name = dbClient.GetString();
        }
        if (string.IsNullOrEmpty(name))
            name = "Unknown User";
        return name;
    }

    public static Habbo GetHabboById(int userId)
    {
        try
        {
            var client = GetGame().GetClientManager().GetClientByUserId(userId);
            if (client != null)
            {
                var user = client.GetHabbo();
                if (user != null && user.Id > 0)
                {
                    if (_usersCached.ContainsKey(userId))
                        _usersCached.TryRemove(userId, out user);
                    return user;
                }
            }
            else
            {
                try
                {
                    if (_usersCached.ContainsKey(userId))
                        return _usersCached[userId];
                    var data = UserDataFactory.GetUserData(userId);
                    if (data != null)
                    {
                        var generated = data.User;
                        if (generated != null)
                        {
                            generated.InitInformation(data);
                            _usersCached.TryAdd(userId, generated);
                            return generated;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static Habbo GetHabboByUsername(string userName)
    {
        try
        {
            using var dbClient = GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
            dbClient.AddParameter("user", userName);
            var id = dbClient.GetInteger();
            if (id > 0)
                return GetHabboById(Convert.ToInt32(id));
            return null;
        }
        catch
        {
            return null;
        }
    }


    public static void PerformShutDown()
    {
        Console.Clear();
        Log.Info("Server shutting down...");
        Console.Title = "PLUS EMULATOR: SHUTTING DOWN!";
        GetGame().GetClientManager().SendPacket(new BroadcastMessageAlertComposer(GetLanguageManager().TryGetValue("server.shutdown.message")));
        GetGame().StopGameLoop();
        Thread.Sleep(2500);
        _connectionManager.Destroy(); //Stop listening.
        if (GetGame().GetPacketManager() is IDisposable disposable)
            disposable.Dispose();
        GetGame().GetClientManager().CloseAll(); //Close all connections
        GetGame().GetRoomManager().Dispose(); //Stop the game loop.
        if (!Debugger.IsAttached)
        {
            using (var dbClient = _database.GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET `online` = '0', `auth_ticket` = NULL");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }
        }
        Log.Info("Plus Emulator has successfully shutdown.");
        Thread.Sleep(1000);
        Environment.Exit(0);
    }

    public static ConfigurationData GetConfig() => _configuration;

    public static Encoding GetDefaultEncoding() => _defaultEncoding;

    public static IGame GetGame() => _game;

    public static IRconSocket GetRconSocket() => _rcon;

    public static IFigureDataManager GetFigureManager() => _figureManager;

    public static IDatabase GetDatabaseManager() => _database;

    public static ILanguageManager GetLanguageManager() => _languageManager;

    public static ISettingsManager GetSettingsManager() => _settingsManager;

    public static ICollection<Habbo> GetUsersCached() => _usersCached.Values;

    public static bool RemoveFromCache(int id, out Habbo data) => _usersCached.TryRemove(id, out data);
}