// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBrokerConnectionSettings.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An interface that contains the connection settings for the grains to synchronize the data to other brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces
{
    /// <summary>
    /// An interface that contains the connection settings for the grains to synchronize the data to other brokers.
    /// </summary>
    public interface IBrokerConnectionSettings
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TLS should be used or not.
        /// </summary>
        bool UseTls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a clean session should be used or not.
        /// </summary>
        bool UseCleanSession { get; set; }
    }
}
