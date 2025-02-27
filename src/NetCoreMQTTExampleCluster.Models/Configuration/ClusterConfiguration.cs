// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="ClusterConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <inheritdoc cref="IConfigurationValid"/>
/// <summary>
/// A class that contains the <see cref="ClusterConfiguration" /> read from the JSON settings file.
/// </summary>
public class ClusterConfiguration : IConfigurationValid
{
    /// <summary>
    /// Gets or sets the log folder path.
    /// </summary>
    public string LogFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the unencrypted port.
    /// </summary>
    public int? UnencryptedPort { get; set; }

    /// <summary>
    /// Gets or sets the heartbeat interval in milliseconds.
    /// </summary>
    public int HeartbeatIntervalInMilliSeconds { get; set; }

    /// <summary>
    /// Gets or sets the service delay in milliseconds.
    /// </summary>
    public int ServiceDelayInMilliSeconds { get; set; }

    /// <summary>
    /// Gets or sets the broker connection settings.
    /// </summary>
    public BrokerConnectionSettings? BrokerConnectionSettings { get; set; }

    /// <summary>
    /// Gets or sets the Orleans configuration.
    /// </summary>
    public OrleansConfiguration? OrleansConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the database settings.
    /// </summary>
    public MqttDatabaseConnectionSettings? DatabaseSettings { get; set; }

    /// <summary>
    /// Gets or sets the JSON web token configuration key.
    /// </summary>
    public string JsonWebTokenConfigurationKey { get; set; } = string.Empty;

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(this.LogFolderPath))
        {
            throw new ConfigurationException("The log folder path is empty.");
        }

        if (!this.Port.IsPortValid())
        {
            throw new ConfigurationException("The port is invalid.");
        }

        if (this.HeartbeatIntervalInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The heartbeat interval is set to 0 or less.");
        }

        if (this.ServiceDelayInMilliSeconds <= 0)
        {
            throw new ConfigurationException("The service delay interval is set to 0 or less.");
        }

        if (this.BrokerConnectionSettings is null || !this.BrokerConnectionSettings.IsValid())
        {
            throw new ConfigurationException("The broker connection is invalid.");
        }

        if (this.OrleansConfiguration is null || !this.OrleansConfiguration.IsValid())
        {
            throw new ConfigurationException("The Orleans configuration is invalid.");
        }

        if (this.DatabaseSettings is null || !this.DatabaseSettings.IsValid())
        {
            throw new ConfigurationException("The database settings are invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.JsonWebTokenConfigurationKey))
        {
            throw new ConfigurationException("The JSON web token configuration key is empty.");
        }

        if (this.JsonWebTokenConfigurationKey.Length < 32)
        {
            throw new ConfigurationException("The JSON WebToken configuration key is too short.");
        }

        return true;
    }
}