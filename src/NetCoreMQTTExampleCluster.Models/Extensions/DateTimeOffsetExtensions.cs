// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension method for the <see cref="DateTimeOffset" /> data type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Extensions;

/// <summary>
/// A class that contains extension method for the <see cref="DateTimeOffset"/> data type.
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Gets the end of the month.
    /// </summary>
    /// <param name="date">The date to get the month end from.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> that represents the end of the month.</returns>
    public static DateTimeOffset EndOfMonth(this DateTimeOffset date)
    {
        var lastDayInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        var newDate = new DateTime(date.Year, date.Month, lastDayInMonth, 23, 59, 59);
        var timeZoneOffset = newDate.GetTimeZoneOffset();
        return new DateTimeOffset(newDate, timeZoneOffset);
    }

    /// <summary>
    /// Checks for an expired date.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>True when expired otherwise false.</returns>
    public static bool IsExpired(this DateTimeOffset timestamp, TimeSpan duration)
    {
        return timestamp.Add(duration) < DateTimeOffset.Now;
    }
}
