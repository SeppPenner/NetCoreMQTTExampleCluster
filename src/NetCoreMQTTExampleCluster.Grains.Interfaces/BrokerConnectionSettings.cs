// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the connection settings for the grains to synchronize the data to other brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces
{
    /// <inheritdoc cref="IBrokerConnectionSettings"/>
    /// <summary>
    /// A class that contains the connection settings for the grains to synchronize the data to other brokers.
    /// </summary>
    /// <seealso cref="IBrokerConnectionSettings"/>
    public class BrokerConnectionSettings : IBrokerConnectionSettings
    {
        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public string ClientId { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public string HostName { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public int Port { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public string UserName { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public string Password { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets a value indicating whether TLS should be used or not.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public bool UseTls { get; set; }

        /// <inheritdoc cref="IBrokerConnectionSettings"/>
        /// <summary>
        /// Gets or sets a value indicating whether a clean session should be used or not.
        /// </summary>
        /// <seealso cref="IBrokerConnectionSettings"/>
        public bool UseCleanSession { get; set; }
    }
}
