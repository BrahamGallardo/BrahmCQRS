namespace BrahmCQRS.Application.Contracts.Services;

/// <summary>
/// Service contract for providing time-related operations.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time.</returns>
    DateTime GetUtcNow();

    /// <summary>
    /// Gets the current date and time in the configured server timezone.
    /// </summary>
    /// <returns>The current server date and time.</returns>
    DateTime GetServerTime();

    /// <summary>
    /// Converts a UTC time to the configured server timezone.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert.</param>
    /// <returns>The time converted to server timezone.</returns>
    DateTime ConvertToServerTime(DateTime utcTime);

    /// <summary>
    /// Converts a server time to UTC.
    /// </summary>
    /// <param name="serverTime">The server time to convert.</param>
    /// <returns>The time converted to UTC.</returns>
    DateTime ConvertToUtc(DateTime serverTime);
}
