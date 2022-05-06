using System;
using System.Threading;
using NLog;
using Plus.Communication.Packets.Outgoing.Handshake;

namespace Plus.HabboHotel.Users.Process;

internal sealed class ProcessComponent
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.HabboHotel.Users.Process.ProcessComponent");

    /// <summary>
    /// How often the timer should execute.
    /// </summary>
    private static readonly int _runtimeInSec = 60;

    /// <summary>
    /// Used for disposing the ProcessComponent safely.
    /// </summary>
    private readonly AutoResetEvent _resetEvent = new(true);

    /// <summary>
    /// Enable/Disable the timer WITHOUT disabling the timer itself.
    /// </summary>
    private bool _disabled;

    /// <summary>
    /// Player to update, handle, change etc.
    /// </summary>
    private Habbo _player;

    /// <summary>
    /// ThreadPooled Timer.
    /// </summary>
    private Timer _timer;

#pragma warning disable CS0414 // The field 'ProcessComponent._timerLagging' is assigned but its value is never used
    /// <summary>
    /// Checks if the timer is lagging behind (server can't keep up).
    /// </summary>
    private bool _timerLagging;
#pragma warning restore CS0414 // The field 'ProcessComponent._timerLagging' is assigned but its value is never used

    /// <summary>
    /// Prevents the timer from overlapping itself.
    /// </summary>
    private bool _timerRunning;

    /// <summary>
    /// Initializes the ProcessComponent.
    /// </summary>
    /// <param name="player">Player.</param>
    public bool Init(Habbo player)
    {
        if (player == null)
            return false;
        if (_player != null)
            return false;
        _player = player;
        _timer = new Timer(Run, null, _runtimeInSec * 1000, _runtimeInSec * 1000);
        return true;
    }

    /// <summary>
    /// Called for each time the timer ticks.
    /// </summary>
    /// <param name="state"></param>
    public void Run(object state)
    {
        try
        {
            if (_disabled)
                return;
            if (_timerRunning)
            {
                _timerLagging = true;
                Log.Warn("<Player " + _player.Id + "> Server can't keep up, Player timer is lagging behind.");
                return;
            }
            _resetEvent.Reset();

            // BEGIN CODE
            if (_player.TimeMuted > 0)
                _player.TimeMuted -= 60;
            if (_player.MessengerSpamTime > 0)
                _player.MessengerSpamTime -= 60;
            if (_player.MessengerSpamTime <= 0)
                _player.MessengerSpamCount = 0;
            _player.TimeAfk += 1;
            if (_player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
            {
                _player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_statistics` SET `dailyRespectPoints` = '" + (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20) +
                                      "', `dailyPetRespectPoints` = '" + (_player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20) + "', `respectsTimestamp` = '" +
                                      DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + _player.Id + "' LIMIT 1");
                }
                _player.GetStats().DailyRespectPoints = _player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20;
                _player.GetStats().DailyPetRespectPoints = _player.Rank == 1 && _player.VipRank == 0 ? 10 : _player.VipRank == 1 ? 15 : 20;
                if (_player.GetClient() != null) _player.GetClient().SendPacket(new UserObjectComposer(_player));
            }
            if (_player.GiftPurchasingWarnings < 15)
                _player.GiftPurchasingWarnings = 0;
            if (_player.MottoUpdateWarnings < 15)
                _player.MottoUpdateWarnings = 0;
            if (_player.ClothingUpdateWarnings < 15)
                _player.ClothingUpdateWarnings = 0;
            if (_player.GetClient() != null)
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(_player.GetClient(), "ACH_AllTimeHotelPresence", 1);
            _player.CheckCreditsTimer();
            _player.Effects().CheckEffectExpiry(_player);

            // END CODE

            // Reset the values
            _timerRunning = false;
            _timerLagging = false;
            _resetEvent.Set();
        }
        catch { }
    }

    /// <summary>
    /// Stops the timer and disposes everything.
    /// </summary>
    public void Dispose()
    {
        // Wait until any processing is complete first.
        try
        {
            _resetEvent.WaitOne(TimeSpan.FromMinutes(5));
        }
        catch { } // give up

        // Set the timer to disabled
        _disabled = true;

        // Dispose the timer to disable it.
        try
        {
            if (_timer != null)
                _timer.Dispose();
        }
        catch { }

        // Remove reference to the timer.
        _timer = null;

        // Null the player so we don't reference it here anymore
        _player = null;
    }
}