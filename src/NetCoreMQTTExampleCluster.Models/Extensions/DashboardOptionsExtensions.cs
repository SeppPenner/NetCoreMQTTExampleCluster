// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardOptionsExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension methods for the <see cref="DashboardOptions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Extensions;

/// <summary>
/// A class that contains extension methods for the <see cref="DashboardOptions"/> class.
/// </summary>
public static class DashboardOptionsExtensions
{
    /// <summary>
    /// Binds the read endpoint options to the orleans options.
    /// </summary>
    /// <param name="options">The Orleans options.</param>
    /// <param name="readOptions">The options from the configuration.</param>
    public static void Bind(this DashboardOptions options, DashboardOptions readOptions)
    {
        options.CounterUpdateIntervalMs = readOptions.CounterUpdateIntervalMs;
        options.HideTrace = readOptions.HideTrace;
        options.HistoryLength = readOptions.HistoryLength;
    }
}