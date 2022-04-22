// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrleansConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Configuration;

/// <inheritdoc cref="IConfigurationValid"/>
/// <summary>
/// A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
/// </summary>
public class OrleansConfiguration : IConfigurationValid
{
    /// <summary>
    /// Gets or sets the cluster options.
    /// </summary>
    public ClusterOptions? ClusterOptions { get; set; }

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
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

        return true;
    }
}
