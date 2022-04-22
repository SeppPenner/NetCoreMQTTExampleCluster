// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrleansConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost.Configuration;

/// <summary>
///  A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
/// </summary>
public class OrleansConfiguration
{
    /// <summary>
    /// Gets or sets the cluster options.
    /// </summary>
    public ClusterOptions ClusterOptions { get; set; }

    /// <summary>
    /// Gets or sets the dashboard options.
    /// </summary>
    public DashboardOptions DashboardOptions { get; set; }

    /// <summary>
    /// Gets or sets the Orleans endpoint options.
    /// </summary>
    public SiloEndpointOptions EndpointOptions { get; set; }
}