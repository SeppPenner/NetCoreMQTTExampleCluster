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
public class SiloHostConfiguration : IConfigurationValid
{
    /// <summary>
    /// Gets or sets the log folder path.
    /// </summary>
    public string LogFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the heartbeat interval in milliseconds.
    /// </summary>
    public int HeartbeatIntervalInMilliSeconds { get; set; }

    /// <summary>
    /// Gets or sets the database settings.
    /// </summary>
    public MqttDatabaseConnectionSettings? DatabaseSettings { get; set; }

    /// <summary>
    /// Gets or sets the Orleans configuration.
    /// </summary>
    public OrleansSiloConfiguration? OrleansConfiguration { get; set; }

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(this.LogFolderPath))
        {
            throw new ConfigurationException("The log folder path is empty.");
        }

        if (this.HeartbeatIntervalInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The heartbeat interval is set to 0 or less.");
        }

        if (this.DatabaseSettings is null || !this.DatabaseSettings.IsValid())
        {
            throw new ConfigurationException("The database settings are invalid.");
        }

        if (this.OrleansConfiguration is null || !this.OrleansConfiguration.IsValid())
        {
            throw new ConfigurationException("The Orleans configuration is invalid.");
        }

        return true;
    }
}