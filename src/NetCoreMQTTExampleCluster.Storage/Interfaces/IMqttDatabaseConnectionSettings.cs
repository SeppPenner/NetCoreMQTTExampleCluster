// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttDatabaseConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface for the database connection settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Interfaces;

/// <summary>
/// An interface for the database connection settings.
/// </summary>
public interface IMqttDatabaseConnectionSettings
{
    /// <summary>
    /// Gets or sets the host of the database.
    /// </summary>
    string Host { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    string Database { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    string Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    string Password { get; set; }

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    int Port { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pooling is enabled or not.
    /// </summary>
    bool Pooling { get; set; }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    string Timezone { get; set; }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    /// <returns>The connection string from the connection settings.</returns>
    string ToConnectionString();

    /// <summary>
    /// Gets the administrator connection string.
    /// </summary>
    /// <returns>The administrator connection string from the connection settings.</returns>
    string ToAdminConnectionString();
}
