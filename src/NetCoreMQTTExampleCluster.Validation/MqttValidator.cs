// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttValidator.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class to validate the different MQTT contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;

    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Models.Extensions;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    using Serilog;

    /// <inheritdoc cref="IMqttValidator"/>
    /// <summary>
    /// A class to validate the different MQTT contexts.
    /// </summary>
    /// <seealso cref="IMqttValidator"/>
    public class MqttValidator : IMqttValidator
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger Logger = Log.ForContext<MqttValidator>();

        /// <inheritdoc cref="IMqttValidator"/>
        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        /// <seealso cref="IMqttValidator"/>
        public async Task<bool> ValidateConnection(SimpleMqttConnectionValidatorContext context, IUserRepository userRepository, IDictionary<string, User> users, IPasswordHasher<User> passwordHasher)
        {
            Logger.Information("Executed ValidateConnection with parameters: {context}, {users}", context, users);

            var currentUser = await userRepository.GetUserByName(context.UserName).ConfigureAwait(false);

            Logger.Information("Current user is {currentUser}.", currentUser);

            if (currentUser == null)
            {
                Logger.Information("Current user was null.");
                return false;
            }

            if (context.UserName != currentUser.UserName)
            {
                Logger.Information("User name in context doesn't match the current user.");
                return false;
            }

            var hashingResult = passwordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash, context.Password);

            if (hashingResult == PasswordVerificationResult.Failed)
            {
                Logger.Information("Password verification failed.");
                return false;
            }

            if (!currentUser.ValidateClientId)
            {
                users[context.ClientId] = currentUser;
                Logger.Information("Connection valid for {clientId} and {user}.", context.ClientId, currentUser);
                return true;
            }

            if (string.IsNullOrWhiteSpace(currentUser.ClientIdPrefix))
            {
                if (context.ClientId != currentUser.ClientId)
                {
                    Logger.Information("Client id in context doesn't match the current user's client id.");
                    return false;
                }

                users[context.ClientId] = currentUser;
                Logger.Information("Connection valid for {clientId} and {user} when client id prefix was null.", context.ClientId, currentUser);
            }
            else
            {
                users[currentUser.ClientIdPrefix] = currentUser;
                Logger.Information("Connection valid for {clientIdPrefix} and {user}  when client id prefix was not null.", currentUser.ClientIdPrefix, currentUser);
            }

            return true;
        }

        /// <inheritdoc cref="IMqttValidator"/>
        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        /// <seealso cref="IMqttValidator"/>
        public async Task<bool> ValidatePublish(MqttApplicationMessageInterceptorContext context, IUserRepository userRepository, IDictionary<string, User> users, IMemoryCache dataLimitCacheMonth)
        {
            Logger.Information("Executed ValidatePublish with parameters: {context}, {users}", context, users);

            var clientIdPrefix = await GetClientIdPrefix(context.ClientId, userRepository);

            Logger.Information("Client id prefix is {clientIdPrefix}.", clientIdPrefix);

            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = users.TryGetValue(context.ClientId, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was null.", userFound, currentUser);
            }
            else
            {
                userFound = users.TryGetValue(clientIdPrefix, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was not null.", userFound, currentUser);
            }

            if (currentUser == null)
            {
                Logger.Information("Current user was null.");
            }

            if (!userFound || currentUser == null)
            {
                return false;
            }

            var topic = context.ApplicationMessage.Topic;

            Logger.Information("Topic was {topic}.", topic);

            if (currentUser.ThrottleUser)
            {
                var payload = context.ApplicationMessage?.Payload;

                if (payload != null)
                {
                    if (currentUser.MonthlyByteLimit != null)
                    {
                        if (IsUserThrottled(context.ClientId, payload.Length, currentUser.MonthlyByteLimit.Value, dataLimitCacheMonth))
                        {
                            Logger.Information("User is throttled now.");
                            return false;
                        }
                    }
                }
            }

            // Get blacklist
            var publishBlackList = await userRepository.GetBlacklistItemsForUser(currentUser.Id, BlacklistWhitelistType.Publish);
            var blacklist = publishBlackList?.ToList() ?? new List<BlacklistWhitelist>();

            Logger.Information("The blacklist was {blacklist}.", blacklist);

            // Get whitelist
            var publishWhitelist = await userRepository.GetWhitelistItemsForUser(currentUser.Id, BlacklistWhitelistType.Publish);
            var whitelist = publishWhitelist?.ToList() ?? new List<BlacklistWhitelist>();

            Logger.Information("The whitelist was {whitelist}.", whitelist);

            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                Logger.Information("The blacklist matched a topic.");
                return false;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
                Logger.Information("The whitelist matched a topic.");
                return true;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var forbiddenTopic in blacklist)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                Logger.Information("The blacklist matched a topic with regex.");
                return false;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var allowedTopic in whitelist)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                Logger.Information("The whitelist matched a topic with regex.");
                return true;
            }

            Logger.Information("We fell through everything else. This should never happen!");
            return false;
        }

        /// <inheritdoc cref="IMqttValidator"/>
        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        /// <seealso cref="IMqttValidator"/>
        public async Task<bool> ValidateSubscription(MqttSubscriptionInterceptorContext context, IUserRepository userRepository, IDictionary<string, User> users)
        {
            Logger.Information("Executed ValidateSubscription with parameters: {context}, {users}", context, users);

            var clientIdPrefix = await GetClientIdPrefix(context.ClientId, userRepository);

            Logger.Information("Client id prefix is {clientIdPrefix}.", clientIdPrefix);

            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = users.TryGetValue(context.ClientId, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was null.", userFound, currentUser);
            }
            else
            {
                userFound = users.TryGetValue(clientIdPrefix, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was not null.", userFound, currentUser);
            }

            if (currentUser == null)
            {
                Logger.Information("Current user was null.");
            }

            if (!userFound || currentUser == null)
            {
                return false;
            }

            var topic = context.TopicFilter.Topic;

            Logger.Information("Topic was {topic}.", topic);

            // Get blacklist
            var subscriptionBlacklist = await userRepository.GetBlacklistItemsForUser(currentUser.Id, BlacklistWhitelistType.Subscribe);
            var blacklist = subscriptionBlacklist?.ToList() ?? new List<BlacklistWhitelist>();

            Logger.Information("The blacklist was {blacklist}.", blacklist);

            // Get whitelist
            var subscriptionWhitelist = await userRepository.GetWhitelistItemsForUser(currentUser.Id, BlacklistWhitelistType.Subscribe);
            var whitelist = subscriptionWhitelist?.ToList() ?? new List<BlacklistWhitelist>();

            Logger.Information("The whitelist was {whitelist}.", whitelist);

            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                Logger.Information("The blacklist matched a topic.");
                return false;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
                Logger.Information("The whitelist matched a topic.");
                return true;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var forbiddenTopic in blacklist)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                Logger.Information("The blacklist matched a topic with regex.");
                return false;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var allowedTopic in whitelist)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

                if (!doesTopicMatch)
                {
                    continue;
                }

                Logger.Information("The whitelist matched a topic with regex.");
                return true;
            }

            Logger.Information("We fell through everything else. This should never happen!");
            return false;
        }

        /// <inheritdoc cref="IMqttValidator"/>
        /// <summary>
        ///     Checks whether the user is a user used for synchronization.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="users">The users.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        /// <seealso cref="IMqttValidator"/>
        public async Task<bool> IsUserBrokerUser(string clientId, IUserRepository userRepository, IDictionary<string, User> users)
        {
            Logger.Information("Executed IsUserBrokerUser with parameters: {clientId}, {users}", clientId, users);

            var clientIdPrefix = await GetClientIdPrefix(clientId, userRepository);

            Logger.Information("Client id prefix is {clientIdPrefix}.", clientIdPrefix);

            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = users.TryGetValue(clientId, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was null.", userFound, currentUser);
            }
            else
            {
                userFound = users.TryGetValue(clientIdPrefix, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;

                Logger.Information("User was found: {userFound}, Current user was {currentUser} when client id prefix was not null.", userFound, currentUser);
            }

            if (currentUser == null)
            {
                Logger.Information("Current user was null.");
            }

            if (!userFound || currentUser == null)
            {
                return false;
            }

            return currentUser.IsSyncUser;
        }

        /// <summary>
        ///     Gets the client identifier prefix for a client identifier if there is one or <c>null</c> else.
        /// </summary>
        /// <param name="clientIdentifierParam">The client identifier.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <returns>The client identifier prefix for a client identifier if there is one or <c>null</c> else.</returns>
        private static async Task<string> GetClientIdPrefix(string clientIdentifierParam, IUserRepository userRepository)
        {
            Logger.Information("The client id parameter was {clientIdentifierParam}.", clientIdentifierParam);

            var clientIdPrefixes = await userRepository.GetAllClientIdPrefixes();
            Logger.Information("The client id prefixes were {clientIdPrefixes}.", clientIdPrefixes);

            var firstOrDefaultClientIdPrefix = clientIdPrefixes.FirstOrDefault(clientIdentifierParam.StartsWith);
            Logger.Information("The first or default client id prefix was {firstOrDefaultClientIdPrefix}.", firstOrDefaultClientIdPrefix);
            return firstOrDefaultClientIdPrefix;
        }

        /// <summary>
        ///     Checks whether a user has used the maximum of its publishing limit for the month or not.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="sizeInBytes">The message size in bytes.</param>
        /// <param name="monthlyByteLimit">The monthly byte limit.</param>
        /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
        /// <returns>A value indicating whether the user will be throttled or not.</returns>
        private static bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit, IMemoryCache dataLimitCacheMonth)
        {
            dataLimitCacheMonth.TryGetValue(clientId, out var foundByteLimit);

            // ReSharper disable once StyleCop.SA1126
            if (foundByteLimit == null)
            {
                dataLimitCacheMonth.Set(clientId, sizeInBytes, DateTimeOffset.Now.EndOfMonth());

                if (sizeInBytes < monthlyByteLimit)
                {
                    return false;
                }

                Logger.Information("The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.", clientId);
                return true;
            }

            try
            {
                var currentValue = Convert.ToInt64(foundByteLimit);
                currentValue = checked(currentValue + sizeInBytes);
                dataLimitCacheMonth.Remove(clientId);
                dataLimitCacheMonth.Set(clientId, currentValue, DateTimeOffset.Now.EndOfMonth());

                if (currentValue >= monthlyByteLimit)
                {
                    Logger.Information("The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.", clientId);
                    return true;
                }
            }
            catch (OverflowException)
            {
                Logger.Information("OverflowException thrown.");
                Logger.Information("The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.", clientId);
                return true;
            }

            return false;
        }
    }
}