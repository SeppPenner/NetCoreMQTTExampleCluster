// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrleansConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <inheritdoc cref="OrleansConfiguration"/>
/// <summary>
///  A class that contains the <see cref="OrleansSiloConfiguration" /> read from the JSON settings file.
/// </summary>
public class OrleansSiloConfiguration : OrleansConfiguration
{
    /// <summary>
    /// Gets or sets the dashboard options.
    /// </summary>
    public DashboardOptions? DashboardOptions { get; set; }

    /// <summary>
    /// Gets or sets the Orleans endpoint options.
    /// </summary>
    public SiloEndpointOptions? EndpointOptions { get; set; }

    /// <inheritdoc cref="IConfigurationValid"/>
    public override bool IsValid()
    {
        if (this.ClusterOptions is null)
        {
            throw new ConfigurationException("The cluster options are empty.");
        }

        if (string.IsNullOrWhiteSpace(this.ClusterOptions.ClusterId))
        {
            throw new ConfigurationException("The cluster identifier is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.ClusterOptions.ServiceId))
        {
            throw new ConfigurationException("The service identifier is empty.");
        }

        if (this.DashboardOptions is null)
        {
            throw new ConfigurationException("The dashboard options are empty.");
        }

        if (this.EndpointOptions is null || !this.EndpointOptions.IsValid())
        {
            throw new ConfigurationException("The dashboard options are empty.");
        }

        return base.IsValid();
    }
}