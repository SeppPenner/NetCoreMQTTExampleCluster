// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterConfiguration.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="ClusterConfiguration" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster.Configuration
{

    using NetCoreMQTTExampleCluster.Models;
    using NetCoreMQTTExampleCluster.Storage;

    /// <summary>
    ///     A class that contains the <see cref="ClusterConfiguration" /> read from the JSON settings file.
    /// </summary>
    public class ClusterConfiguration
    {
        /// <summary>
        /// Gets or sets the log folder path.
        /// </summary>
        public string LogFolderPath { get; set; }

        /// <summary>
        ///     Gets or sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     Gets or sets the unencrypted port.
        /// </summary>
        public int? UnencryptedPort { get; set; }

        /// <summary>
        /// Gets or sets the heartbeat interval in milliseconds.
        /// </summary>
        public int HeartbeatIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the broker connection settings.
        /// </summary>
        public BrokerConnectionSettings BrokerConnectionSettings { get; set; }

        /// <summary>
        /// Gets or sets the Orleans configuration.
        /// </summary>
        public OrleansConfiguration OrleansConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the database settings.
        /// </summary>
        public MqttDatabaseConnectionSettings DatabaseSettings { get; set; }
    }
}