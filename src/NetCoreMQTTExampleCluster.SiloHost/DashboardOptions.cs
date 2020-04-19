// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardOptions.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the Orleans dashboard settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost
{
    /// <summary>
    /// A class that contains the Orleans dashboard settings.
    /// </summary>
    public class DashboardOptions
    {
        /// <summary>
        /// Gets or sets the counter update interval in milliseconds.
        /// </summary>
        public int CounterUpdateIntervalMs { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; }
    }
}