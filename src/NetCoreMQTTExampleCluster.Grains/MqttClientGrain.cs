// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttClientGrain.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The grain for one client identifier.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;

    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Models.Extensions;
    using NetCoreMQTTExampleCluster.TopicCheck;

    using Orleans;

    using Serilog;

    /// <inheritdoc cref="IMqttClientGrain" />
    /// <summary>
    ///     The grain for one client identifier.
    /// </summary>
    /// <seealso cref="IMqttClientGrain" />
    public class MqttClientGrain : Grain, IMqttClientGrain
    {
        /// <summary>
        ///     Gets or sets the data limit cache for throttling for monthly data.
        /// </summary>
        private static readonly MemoryCache DataLimitCacheMonth = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        ///     The <see cref="PasswordHasher{TUser}" />.
        /// </summary>
        private static readonly PasswordHasher<User> Hasher = new PasswordHasher<User>();

        /// <summary>
        ///     The user repository.
        /// </summary>
        private readonly IUserRepository userRepository;

        /// <summary>
        ///     The session items.
        /// </summary>
        private readonly IDictionary<string, User> users = new ConcurrentDictionary<string, User>();

        /// <summary>
        ///     The client identifier.
        /// </summary>
        private string clientId = string.Empty;

        /// <summary>
        ///     The logger.
        /// </summary>
        private ILogger log;

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttClientGrain" /> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <seealso cref="IMqttClientGrain" />
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public MqttClientGrain(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            this.log = Log.ForContext("Grain", nameof(MqttClientGrain));
        }

        /// <inheritdoc cref="Grain" />
        /// <summary>
        ///     This method is called at the end of the process of activating a grain.
        ///     It is called before any messages have been dispatched to the grain.
        ///     For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="Grain" />
        public override Task OnActivateAsync()
        {
            this.clientId = this.GetPrimaryKeyString();
            this.log = Log.ForContext("Grain", nameof(MqttClientGrain)).ForContext("Id", this.clientId);
            return base.OnActivateAsync();
        }

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        ///     Proceeds the connection for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        /// <seealso cref="IMqttClientGrain" />
        public async Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context)
        {
            try
            {
                return await this.ValidateConnection(context);
            }
            catch (Exception ex)
            {
                this.log.Error(ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        ///     Proceeds the published message for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        /// <seealso cref="IMqttClientGrain" />
        public async Task<bool> ProceedPublish(MqttApplicationMessageInterceptorContext context)
        {
            try
            {
                return await this.ValidatePublish(context);
            }
            catch (Exception ex)
            {
                this.log.Error(ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        ///     Proceeds the subscription for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        /// <seealso cref="IMqttClientGrain" />
        public async Task<bool> ProceedSubscription(MqttSubscriptionInterceptorContext context)
        {
            try
            {
                return await this.ValidateSubscription(context);
            }
            catch (Exception ex)
            {
                this.log.Error(ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Proceeds the disconnection message for one client identifier.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        public void ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            try
            {
                this.users.Remove(eventArgs.ClientId);
            }
            catch (Exception ex)
            {
                this.log.Error(ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        ///     Checks whether a user has used the maximum of its publishing limit for the month or not.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="sizeInBytes">The message size in bytes.</param>
        /// <param name="monthlyByteLimit">The monthly byte limit.</param>
        /// <returns>A value indicating whether the user will be throttled or not.</returns>
        private static bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit)
        {
            DataLimitCacheMonth.TryGetValue(clientId, out var foundByteLimit);

            // ReSharper disable once StyleCop.SA1126
            if (foundByteLimit == null)
            {
                DataLimitCacheMonth.Set(clientId, sizeInBytes, DateTimeOffset.Now.EndOfMonth());

                if (sizeInBytes < monthlyByteLimit)
                {
                    return false;
                }

                Log.Information($"The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.");
                return true;
            }

            try
            {
                var currentValue = Convert.ToInt64(foundByteLimit);
                currentValue = checked(currentValue + sizeInBytes);
                DataLimitCacheMonth.Remove(clientId);
                DataLimitCacheMonth.Set(clientId, currentValue, DateTimeOffset.Now.EndOfMonth());

                if (currentValue >= monthlyByteLimit)
                {
                    Log.Information($"The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.");
                    return true;
                }
            }
            catch (OverflowException)
            {
                Log.Information("OverflowException thrown.");
                Log.Information($"The client with client identifier {clientId} is now locked until the end of this month because it already used its data limit.");
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the client identifier prefix for a client identifier if there is one or <c>null</c> else.
        /// </summary>
        /// <param name="clientIdentifierParam">The client identifier.</param>
        /// <returns>The client identifier prefix for a client identifier if there is one or <c>null</c> else.</returns>
        private async Task<string> GetClientIdPrefix(string clientIdentifierParam)
        {
            var clientIdPrefixes = await this.userRepository.GetAllClientIdPrefixes();
            return clientIdPrefixes.FirstOrDefault(clientIdentifierParam.StartsWith);
        }

        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        private async Task<bool> ValidateConnection(SimpleMqttConnectionValidatorContext context)
        {
            var currentUser = await this.userRepository.GetUserByName(context.UserName).ConfigureAwait(false);

            if (currentUser == null)
            {
                return false;
            }

            if (context.UserName != currentUser.UserName)
            {
                return false;
            }

            var hashingResult = Hasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash, context.Password);

            if (hashingResult == PasswordVerificationResult.Failed)
            {
                return false;
            }

            if (!currentUser.ValidateClientId)
            {
                this.users[context.ClientId] = currentUser;
                return true;
            }

            if (string.IsNullOrWhiteSpace(currentUser.ClientIdPrefix))
            {
                if (context.ClientId != currentUser.ClientId)
                {
                    return false;
                }

                this.users[context.ClientId] = currentUser;
            }
            else
            {
                this.users[currentUser.ClientIdPrefix] = currentUser;
            }

            return true;
        }

        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        private async Task<bool> ValidatePublish(MqttApplicationMessageInterceptorContext context)
        {
            var clientIdPrefix = await this.GetClientIdPrefix(context.ClientId);
            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = this.users.TryGetValue(context.ClientId, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;
            }
            else
            {
                userFound = this.users.TryGetValue(clientIdPrefix, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;
            }

            if (!userFound || currentUser == null)
            {
                return false;
            }

            var topic = context.ApplicationMessage.Topic;

            if (currentUser.ThrottleUser)
            {
                var payload = context.ApplicationMessage?.Payload;

                if (payload != null)
                {
                    if (currentUser.MonthlyByteLimit != null)
                    {
                        if (IsUserThrottled(context.ClientId, payload.Length, currentUser.MonthlyByteLimit.Value))
                        {
                            return false;
                        }
                    }
                }
            }

            // Get blacklist
            var publishBlackList = await this.userRepository.GetBlacklistItemsForUser(currentUser.Id, BlacklistWhitelistType.Publish);
            var blacklist = publishBlackList?.ToList() ?? new List<BlacklistWhitelist>();

            // Get whitelist
            var publishWhitelist = await this.userRepository.GetWhitelistItemsForUser(currentUser.Id, BlacklistWhitelistType.Publish);
            var whitelist = publishWhitelist?.ToList() ?? new List<BlacklistWhitelist>();

            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                return false;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
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

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        private async Task<bool> ValidateSubscription(MqttSubscriptionInterceptorContext context)
        {
            var clientIdPrefix = await this.GetClientIdPrefix(context.ClientId);
            User currentUser;
            bool userFound;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                userFound = this.users.TryGetValue(context.ClientId, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;
            }
            else
            {
                userFound = this.users.TryGetValue(clientIdPrefix, out var currentUserObject);

                // ReSharper disable once StyleCop.SA1126
                currentUser = currentUserObject;
            }

            if (!userFound || currentUser == null)
            {
                return false;
            }

            var topic = context.TopicFilter.Topic;

            // Get blacklist
            var publishBlackList = await this.userRepository.GetBlacklistItemsForUser(currentUser.Id, BlacklistWhitelistType.Subscribe);
            var blacklist = publishBlackList?.ToList() ?? new List<BlacklistWhitelist>();

            // Get whitelist
            var publishWhitelist = await this.userRepository.GetWhitelistItemsForUser(currentUser.Id, BlacklistWhitelistType.Subscribe);
            var whitelist = publishWhitelist?.ToList() ?? new List<BlacklistWhitelist>();

            // Check matches
            if (blacklist.Any(b => b.Value == topic))
            {
                return false;
            }

            if (whitelist.Any(b => b.Value == topic))
            {
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

                return true;
            }

            return false;
        }
    }
}
