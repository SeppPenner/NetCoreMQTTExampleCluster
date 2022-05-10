// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension method for the <see cref="DateTime" /> data type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Extensions;

/// <summary>
/// A class that contains extension method for the <see cref="DateTime"/> data type.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the time zone offset of the local time zone.
    /// </summary>
    /// <param name="date">The date to get the time zone offset from.</param>
    /// <returns>The time zone offset of the local time zone</returns>
    public static TimeSpan GetTimeZoneOffset(this DateTime date)
    {
        return TimeZoneInfo.Local.IsDaylightSavingTime(date) ? TimeSpan.FromHours(2) : TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Checks for an expired date.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>True when expired otherwise false.</returns>
    public static bool IsExpired(this DateTime timestamp, TimeSpan duration)
    {
        return timestamp.Add(duration) < DateTimeOffset.Now;
    }
}
