// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloHostConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="SiloHostConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost.Configuration
{
    using NetCoreMQTTExampleCluster.Storage;

    /// <summary>
    /// A class that contains the <see cref="SiloHostConfiguration" /> read from the JSON settings file.
    /// </summary>
    public class SiloHostConfiguration
    {
        /// <summary>
        /// Gets or sets the log folder path.
        /// </summary>
        public string LogFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the heartbeat interval in milliseconds.
        /// </summary>
        public int HeartbeatIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the database settings.
        /// </summary>
        public MqttDatabaseConnectionSettings DatabaseSettings { get; set; }

        /// <summary>
        /// Gets or sets the Orleans configuration.
        /// </summary>
        public OrleansConfiguration OrleansConfiguration { get; set; }
    }
}