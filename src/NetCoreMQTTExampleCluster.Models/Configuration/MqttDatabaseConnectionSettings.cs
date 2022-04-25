// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttDatabaseConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class for the database connection settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage;

/// <inheritdoc cref="IConfigurationValid"/>
/// <summary>
/// A class for the database connection settings.
/// </summary>
[Serializable]
public class MqttDatabaseConnectionSettings : IConfigurationValid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttDatabaseConnectionSettings"/> class.
    /// </summary>
    public MqttDatabaseConnectionSettings()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttDatabaseConnectionSettings"/> class.
    /// </summary>
    /// <param name="other">The other <see cref="MqttDatabaseConnectionSettings"/>.</param>
    public MqttDatabaseConnectionSettings(MqttDatabaseConnectionSettings other)
    {
        this.Host = other.Host;
        this.Database = other.Database;
        this.Username = other.Username;
        this.Password = other.Password;
        this.Port = other.Port;
        this.Pooling = other.Pooling;
        this.Timezone = other.Timezone;
    }

    /// <summary>
    /// Gets or sets the host of the database.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string Database { get; set; } = "mqtt";

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; } = "postgres";

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = "postgres";

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Gets or sets a value indicating whether the pooling is enabled or not.
    /// </summary>
    public bool Pooling { get; set; }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    public string Timezone { get; set; } = "Europe/Berlin";

    /// <summary>
    /// Gets the connection string.
    /// </summary>
    /// <returns>The connection string from the connection settings.</returns>
    public string ToConnectionString()
    {
        return $"Host={this.Host};Port={this.Port};Username={this.Username};Password={this.Password};Database={this.Database};Pooling={this.Pooling};Timezone={this.Timezone};Enlist=false;Maximum Pool Size=400;ConvertInfinityDateTime=true";
    }

    /// <summary>
    /// Gets the administrator connection string.
    /// </summary>
    /// <returns>The administrator connection string from the connection settings.</returns>
    public string ToAdminConnectionString()
    {
        return $"Host={this.Host};Port={this.Port};Username={this.Username};Password={this.Password};Database=postgres;Pooling={this.Pooling};Timezone={this.Timezone};Enlist=false;Maximum Pool Size=400;ConvertInfinityDateTime=true";
    }

    /// <inheritdoc cref="IConfigurationValid"/>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(this.Host))
        {
            throw new ConfigurationException("The host is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.Database))
        {
            throw new ConfigurationException("The database is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.Username))
        {
            throw new ConfigurationException("The user name is empty.");
        }

        if (string.IsNullOrWhiteSpace(this.Password))
        {
            throw new ConfigurationException("The password is empty.");
        }



        if (!this.Port.IsPortValid())
        {
            throw new ConfigurationException("The port is invalid.");
        }

        if (this.HeartbeatIntervalInMilliseconds <= 0)
        {
            throw new ConfigurationException("The heartbeat interval is set to 0 or less.");
        }

        if (this.BrokerConnectionSettings is null || !this.BrokerConnectionSettings.IsValid())
        {
            throw new ConfigurationException("The broker connection is invalid.");
        }

        if (this.OrleansConfiguration is null || !this.OrleansConfiguration.IsValid())
        {
            throw new ConfigurationException("The Orleans configuration is invalid.");
        }

        if (this.DatabaseSettings is null || !this.DatabaseSettings.IsValid())
        {
            throw new ConfigurationException("The database settings are invalid.");
        }

        return true;
    }
}
