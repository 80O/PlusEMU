namespace Plus.Utilities;

internal static class UnixTimestamp
{
    /// <summary>
    /// Gets the current date time now in Unix Timestamp format.
    /// </summary>
    /// <returns>Unix Timestamp.</returns>
    public static double GetNow()
    {
        var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
        return ts.TotalSeconds;
    }

    /// <summary>
    /// Converts the Unix Timestamp to a DateTime object.
    /// </summary>
    /// <param name="timestamp">Unix Timestamp.</param>
    /// <returns>DateTime object.</returns>
    public static DateTime FromUnixTimestamp(double timestamp)
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        dt = dt.AddSeconds(timestamp);
        return dt;
    }
}