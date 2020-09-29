// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserData.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The user data class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NetCoreMQTTExampleCluster.Storage.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// The user data class.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Gets or sets the subscription whitelist.
        /// </summary>
        public List<BlacklistWhitelist> SubscriptionWhitelist { get; set; }

        /// <summary>
        /// Gets or sets the publish whitelist.
        /// </summary>
        public List<BlacklistWhitelist> PublishWhitelist { get; set; }

        /// <summary>
        /// Gets or sets the subscription blacklist.
        /// </summary>
        public List<BlacklistWhitelist> SubscriptionBlacklist { get; set; }

        /// <summary>
        /// Gets or sets the publish blacklist.
        /// </summary>
        public List<BlacklistWhitelist> PublishBlacklist { get; set; }

        /// <summary>
        /// Gets or sets the client identifier prefixes.
        /// </summary>
        public List<string> ClientIdPrefixes { get; set; }
    }
}
