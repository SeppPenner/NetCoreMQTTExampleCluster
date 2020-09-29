// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttValidator.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A interface to validate the different MQTT contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;

    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;

    /// <summary>
    /// A interface to validate the different MQTT contexts.
    /// </summary>
    public interface IMqttValidator
    {
        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="user">The user.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        bool ValidateConnection(
            SimpleMqttConnectionValidatorContext context,
            User user,
            IPasswordHasher<User> passwordHasher);

        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="blacklist">The blacklist.</param>
        /// <param name="whitelist">The whitelist.</param>
        /// <param name="user">The user.</param>
        /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
        /// <param name="clientIdPrefixes">The client identifier prefixes.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        bool ValidatePublish(
            MqttApplicationMessageInterceptorContext context,
            List<BlacklistWhitelist> blacklist,
            List<BlacklistWhitelist> whitelist,
            User user,
            IMemoryCache dataLimitCacheMonth,
            List<string> clientIdPrefixes);

        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="blacklist">The blacklist.</param>
        /// <param name="whitelist">The whitelist.</param>
        /// <param name="user">The user.</param>
        /// <param name="clientIdPrefixes">The client identifier prefixes.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        bool ValidateSubscription(
            MqttSubscriptionInterceptorContext context,
            List<BlacklistWhitelist> blacklist,
            List<BlacklistWhitelist> whitelist,
            User user,
            List<string> clientIdPrefixes);
    }
}