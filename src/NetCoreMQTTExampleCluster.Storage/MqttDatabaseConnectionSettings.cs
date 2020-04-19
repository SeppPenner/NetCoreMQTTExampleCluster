// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttDatabaseConnectionSettings.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class for the database connection settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage
{
    using System;

    /// <summary>
    /// A class for the database connection settings.
    /// </summary>
    [Serializable]
    public class MqttDatabaseConnectionSettings
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
    }
}