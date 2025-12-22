namespace BrahmCQRS.Shared.TimeZone;

/// <summary>
/// Provides server time in various time zones.
/// </summary>
public static class ServerTimeProvider
{
    /// <summary>
    /// Gets the current server time in Alaskan Standard Time (AKST).
    /// </summary>
    /// <returns>The current date and time in AKST.</returns>
    public static DateTime GetServerTimeAKST() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));

    /// <summary>
    /// Gets the current server time in Pacific Standard Time (PST).
    /// </summary>
    /// <returns>The current date and time in PST.</returns>
    public static DateTime GetServerTimePST() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

    /// <summary>
    /// Gets the current server time in Mountain Standard Time (MST).
    /// </summary>
    /// <returns>The current date and time in MST.</returns>
    public static DateTime GetServerTimeMST() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));

    /// <summary>
    /// Gets the current server time in Central Standard Time (CST) - Mexico.
    /// </summary>
    /// <returns>The current date and time in CST (Mexico).</returns>
    public static DateTime GetServerTimeCST() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));

    /// <summary>
    /// Gets the current server time in Eastern Standard Time (EST).
    /// </summary>
    /// <returns>The current date and time in EST.</returns>
    public static DateTime GetServerTimeEST() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

    /// <summary>
    /// Gets the current server time in Atlantic Standard Time (AST).
    /// Calculated as CST minus 1-2 hours depending on daylight saving time.
    /// </summary>
    /// <returns>The current date and time in AST.</returns>
    public static DateTime GetServerTimeAST()
    {
        var cstTime = GetServerTimeCST();
        return cstTime.IsDaylightSavingTime() ? cstTime.AddHours(-1) : cstTime.AddHours(-2);
    }

    /// <summary>
    /// Gets the current server time in Argentina Standard Time (ART).
    /// Calculated as CST plus 1-2 hours depending on daylight saving time.
    /// </summary>
    /// <returns>The current date and time in ART.</returns>
    public static DateTime GetServerTimeART()
    {
        var cstTime = GetServerTimeCST();
        return cstTime.IsDaylightSavingTime() ? cstTime.AddHours(2) : cstTime.AddHours(1);
    }

    /// <summary>
    /// Gets the current server time in Hawaii Standard Time (HST).
    /// Calculated as CST plus 4-5 hours depending on daylight saving time.
    /// </summary>
    /// <returns>The current date and time in HST.</returns>
    public static DateTime GetServerTimeHST()
    {
        var cstTime = GetServerTimeCST();
        return cstTime.IsDaylightSavingTime() ? cstTime.AddHours(5) : cstTime.AddHours(4);
    }
}
