using BrahmCQRS.Application.Contracts.Services;

namespace BrahmCQRS.Infrastructure.Services;

/// <summary>
/// Implementation of time provider service with configurable timezone.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeProvider"/> class with a specific timezone.
/// </remarks>
/// <param name="serverTimeZone">The server timezone to use.</param>
public class TimeProvider(TimeZoneInfo serverTimeZone) : ITimeProvider
{
    private readonly TimeZoneInfo _serverTimeZone = serverTimeZone ?? throw new ArgumentNullException(nameof(serverTimeZone));

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeProvider"/> class with default CST timezone.
    /// </summary>
    public TimeProvider() : this(GetDefaultTimeZone())
    {
    }

    /// <inheritdoc/>
    public DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public DateTime GetServerTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _serverTimeZone);
    }

    /// <inheritdoc/>
    public DateTime ConvertToServerTime(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Time must be in UTC.", nameof(utcTime));

        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _serverTimeZone);
    }

    /// <inheritdoc/>
    public DateTime ConvertToUtc(DateTime serverTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(serverTime, _serverTimeZone);
    }

    /// <summary>
    /// Gets the default timezone (CST - Central Standard Time Mexico).
    /// Uses fallback if timezone is not found on the current OS.
    /// </summary>
    /// <returns>The default timezone info.</returns>
    private static TimeZoneInfo GetDefaultTimeZone()
    {
        // Windows CST
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Linux / macOS Chicago
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
            }
            catch (TimeZoneNotFoundException)
            {
                
            }
        }
        // Windows Mexico
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        }
        catch (TimeZoneNotFoundException)
        {
            // Linux / macOS Mexico_City
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            }
            catch (TimeZoneNotFoundException)
            {
                
            }
        }
        // Fallback to UTC-6 if neither is found
        return TimeZoneInfo.CreateCustomTimeZone(
            id: "CST-6",
            baseUtcOffset: TimeSpan.FromHours(-6),
            displayName: "Central Standard Time (Custom)",
            standardDisplayName: "Central Standard Time");
    }
}
