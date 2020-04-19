// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the <see cref="Config" /> read from the JSON settings file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models
{
    using System.Diagnostics.CodeAnalysis;

    using NetCoreMQTTExampleCluster.Storage;

    /// <summary>
    ///     A class that contains the <see cref="Config" /> read from the JSON settings file.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class Config
    {
        /// <summary>
        ///     Gets or sets the port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     Gets or sets the unencrypted port.
        /// </summary>
        public int? UnencryptedPort { get; set; }

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