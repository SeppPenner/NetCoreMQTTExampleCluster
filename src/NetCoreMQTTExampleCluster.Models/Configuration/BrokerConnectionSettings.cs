// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the connection settings for the grains to synchronize the data to other brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <inheritdoc cref="IBrokerConnectionSettings"/>
/// <inheritdoc cref="IConfigurationValid"/>
public class BrokerConnectionSettings : IBrokerConnectionSettings, IConfigurationValid
{
    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public string ClientId { get; set; } = string.Empty;

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public string HostName { get; set; } = string.Empty;

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public int Port { get; set; }

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public string UserName { get; set; } = string.Empty;

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public string Password { get; set; } = string.Empty;

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public bool UseTls { get; set; }

    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    public bool UseCleanSession { get; set; }

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(this.ClientId))
        {
            throw new ConfigurationException("The client identifier is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.HostName))
        {
            throw new ConfigurationException("The host name is empty.");
        }

        if (!this.Port.IsPortValid())
        {
            throw new ConfigurationException("The port is invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.UserName))
        {
            throw new ConfigurationException("The user name is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.Password))
        {
            throw new ConfigurationException("The password is empty.");
        }

        return true;
    }
}
