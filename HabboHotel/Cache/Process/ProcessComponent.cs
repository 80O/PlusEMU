using Microsoft.Extensions.Logging;
using Plus.Core;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Cache.Process;

public sealed class ProcessComponent : IProcessComponent
{
    private readonly ILogger<ProcessComponent> _logger;

    public ProcessComponent(ILogger<ProcessComponent> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// How often the timer should execute.
    /// </summary>
    private static readonly int _runtimeInSec = 1200;

    /// <summary>
    /// Used for disposing the ProcessComponent safely.
    /// </summary>
    private readonly AutoResetEvent _resetEvent = new(true);

    /// <summary>
    /// Enable/Disable the timer WITHOUT disabling the timer itself.
    /// </summary>
    private bool _disabled;

    /// <summary>
    /// ThreadPooled Timer.
    /// </summary>
    private Timer _timer;

    /// <summary>
    /// Checks if the timer is lagging behind (server can't keep up).
    /// </summary>
    private bool _timerLagging;

    /// <summary>
    /// Prevents the timer from overlapping itself.
    /// </summary>
    private bool _timerRunning;

    /// <summary>
    /// Initializes the ProcessComponent.
    /// </summary>
    public void Init()
    {
        _timer = new(Run, null, _runtimeInSec * 1000, _runtimeInSec * 1000);
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
                return;
            }
            _resetEvent.Reset();

            // BEGIN CODE
            var cacheList = PlusEnvironment.Game.CacheManager.GetUserCache().ToList();
            if (cacheList.Count > 0)
            {
                foreach (var cache in cacheList)
                {
                    try
                    {
                        if (cache == null)
                            continue;
                        if (cache.IsExpired)
                            PlusEnvironment.Game.CacheManager.TryRemoveUser(cache.Id, out _);
                    }
                    catch (Exception e)
                    {
                        ExceptionLogger.LogException(e);
                    }
                }
            }
            var cachedUsers = PlusEnvironment.CachedUsers.ToList();
            if (cachedUsers.Count > 0)
            {
                foreach (var data in cachedUsers)
                {
                    try
                    {
                        if (data == null)
                            continue;
                        Habbo temp = null;
                        if (data.CacheExpired())
                            PlusEnvironment.RemoveFromCache(data.Id, out temp);
                        if (temp != null)
                            temp.Dispose();
                    }
                    catch (Exception e)
                    {
                        ExceptionLogger.LogException(e);
                    }
                }
            }
            // END CODE

            // Reset the values
            _timerRunning = false;
            _timerLagging = false;
            _resetEvent.Set();
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
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
    }
}