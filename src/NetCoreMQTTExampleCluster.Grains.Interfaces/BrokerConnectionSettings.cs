// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the connection settings for the grains to synchronize the data to other brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <inheritdoc cref="IBrokerConnectionSettings"/>
public class BrokerConnectionSettings : IBrokerConnectionSettings
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
}
