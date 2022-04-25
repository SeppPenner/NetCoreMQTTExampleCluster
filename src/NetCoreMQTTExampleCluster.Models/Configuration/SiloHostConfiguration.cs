// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloHostConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="SiloHostConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <summary>
/// A class that contains the <see cref="SiloHostConfiguration" /> read from the JSON settings file.
/// </summary>
public class SiloHostConfiguration
{
    /// <summary>
    /// Gets or sets the log folder path.
    /// </summary>
    public string LogFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heartbeat interval in milliseconds.
    /// </summary>
    public int HeartbeatIntervalInMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the database settings.
    /// </summary>
    public MqttDatabaseConnectionSettings DatabaseSettings { get; set; }

    /// <summary>
    /// Gets or sets the Orleans configuration.
    /// </summary>
    public OrleansSiloConfiguration OrleansConfiguration { get; set; }
}