// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the connection settings for the grains to synchronize the data to other brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models
{
    using NetCoreMQTTExampleCluster.Grains.Interfaces;

    /// <summary>
    /// A class that contains the connection settings for the grains to synchronize the data to other brokers.
    /// </summary>
    public class BrokerConnectionSettings : IBrokerConnectionSettings
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TLS should be used or not.
        /// </summary>
        public bool UseTls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a clean session should be used or not.
        /// </summary>
        public bool UseCleanSession { get; set; }
    }
}
