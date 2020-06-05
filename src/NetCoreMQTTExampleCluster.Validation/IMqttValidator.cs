// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttValidator.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A interface to validate the different MQTT contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;

    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    /// <summary>
    /// A interface to validate the different MQTT contexts.
    /// </summary>
    public interface IMqttValidator
    {
        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        Task<bool> ValidateConnection(SimpleMqttConnectionValidatorContext context, IUserRepository userRepository, IDictionary<string, User> users, IPasswordHasher<User> passwordHasher);

        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        Task<bool> ValidatePublish(MqttApplicationMessageInterceptorContext context, IUserRepository userRepository, IDictionary<string, User> users, IMemoryCache dataLimitCacheMonth);

        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        Task<bool> ValidateSubscription(MqttSubscriptionInterceptorContext context, IUserRepository userRepository, IDictionary<string, User> users);

        /// <summary>
        ///     Checks whether the user is a user used for synchronization.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        Task<bool> IsUserBrokerUser(string clientId, IUserRepository userRepository, IDictionary<string, User> users);
    }
}