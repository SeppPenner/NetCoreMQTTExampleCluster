// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloEndpointOptions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the Orleans silo endpoint options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <inheritdoc cref="IConfigurationValid"/>
/// <summary>
/// A class that contains the Orleans silo endpoint options.
/// </summary>
public class SiloEndpointOptions : IConfigurationValid
{
    /// <summary>
    /// Gets or sets the advertised IP address.
    /// </summary>
    public string AdvertisedIpAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the silo port.
    /// </summary>
    public int SiloPort { get; set; } = 7101;

    /// <summary>
    /// Gets or sets the gateway port.
    /// </summary>
    public int GatewayPort { get; set; } = 7102;

    /// <summary>
    /// Gets or sets the silo listening endpoint IP address.
    /// </summary>
    public string SiloListeningEndpointAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the silo listening endpoint port.
    /// </summary>
    public int SiloListeningEndpointPort { get; set; } = 7103;

    /// <summary>
    /// Gets or sets the gateway listening endpoint IP address.
    /// </summary>
    public string GatewayListeningEndpointAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the gateway listening endpoint port.
    /// </summary>
    public int GatewayListeningEndpointPort { get; set; } = 7104;

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(this.AdvertisedIpAddress))
        {
            throw new ConfigurationException("The advertised IP address is empty.");
        }

        if (!this.SiloPort.IsPortValid())
        {
            throw new ConfigurationException("The silo port is invalid.");
        }

        if (!this.GatewayPort.IsPortValid())
        {
            throw new ConfigurationException("The gateway port is invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.SiloListeningEndpointAddress))
        {
            throw new ConfigurationException("The silo listening endpoint IP address is empty.");
        }

        if (!this.SiloListeningEndpointPort.IsPortValid())
        {
            throw new ConfigurationException("The silo listening endpoint port is invalid.");
        }

        if (string.IsNullOrWhiteSpace(this.GatewayListeningEndpointAddress))
        {
            throw new ConfigurationException("The gateway listening endpoint IP address is empty.");
        }

        if (!this.GatewayListeningEndpointPort.IsPortValid())
        {
            throw new ConfigurationException("The gateway listening endpoint port is invalid.");
        }

        return true;
    }

    /// <summary>
    /// Binds the options to the object.
    /// </summary>
    /// <param name="options">The endpoint options.</param>
    public void Bind(EndpointOptions options)
    {
        options.AdvertisedIPAddress = IPAddress.Parse(this.AdvertisedIpAddress);
        options.SiloPort = this.SiloPort;
        options.GatewayPort = this.GatewayPort;
        options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Parse(this.SiloListeningEndpointAddress), this.SiloListeningEndpointPort);
        options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Parse(this.GatewayListeningEndpointAddress), this.GatewayListeningEndpointPort);
    }
}
