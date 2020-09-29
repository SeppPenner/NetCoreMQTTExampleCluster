// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrleansConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster.Configuration
{
    using Orleans.Configuration;

    /// <summary>
    /// A class that contains the <see cref="OrleansConfiguration" /> read from the JSON settings file.
    /// </summary>
    public class OrleansConfiguration
    {
        /// <summary>
        ///     Gets or sets the cluster options.
        /// </summary>
        public ClusterOptions ClusterOptions { get; set; }
    }
}
