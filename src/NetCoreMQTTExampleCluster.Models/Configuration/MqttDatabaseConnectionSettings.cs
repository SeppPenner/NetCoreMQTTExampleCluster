// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttDatabaseConnectionSettings.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class for the database connection settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models;

/// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
/// <inheritdoc cref="IConfigurationValid"/>
/// <summary>
/// A class for the database connection settings.
/// </summary>
[Serializable]
public class MqttDatabaseConnectionSettings : IMqttDatabaseConnectionSettings, IConfigurationValid
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

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string Host { get; set; } = "localhost";

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string Database { get; set; } = "mqtt";

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string Username { get; set; } = "postgres";

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string Password { get; set; } = "postgres";

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public int Port { get; set; } = 5432;

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public bool Pooling { get; set; }

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string Timezone { get; set; } = "Europe/Berlin";

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
    public string ToConnectionString()
    {
        return $"Host={this.Host};Port={this.Port};Username={this.Username};Password={this.Password};Database={this.Database};Pooling={this.Pooling};Timezone={this.Timezone};Enlist=false;Maximum Pool Size=400;ConvertInfinityDateTime=true";
    }

    /// <inheritdoc cref="IMqttDatabaseConnectionSettings"/>
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

        if (string.IsNullOrWhiteSpace(this.Timezone))
        {
            throw new ConfigurationException("The time zone is empty.");
        }

        return true;
    }
}
