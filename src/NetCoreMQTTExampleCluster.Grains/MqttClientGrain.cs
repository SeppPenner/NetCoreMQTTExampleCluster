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
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;

    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Validation;

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
        ///     The <see cref="IPasswordHasher{TUser}" />.
        /// </summary>
        private static readonly IPasswordHasher<User> PasswordHasher = new PasswordHasher<User>();

        /// <summary>
        ///     The user repository.
        /// </summary>
        private readonly IUserRepository userRepository;

        /// <summary>
        /// The MQTT validator.
        /// </summary>
        private readonly IMqttValidator mqttValidator;

        /// <summary>
        ///     The users.
        /// </summary>
        private readonly IDictionary<string, User> users = new ConcurrentDictionary<string, User>();

        /// <summary>
        ///     The client identifier.
        /// </summary>
        private string clientId = string.Empty;

        /// <summary>
        ///     The logger.
        /// </summary>
        private ILogger logger;

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttClientGrain" /> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="mqttValidator">The MQTT validator.</param>
        /// <seealso cref="IMqttClientGrain" />
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public MqttClientGrain(IUserRepository userRepository, IMqttValidator mqttValidator)
        {
            this.userRepository = userRepository;
            this.mqttValidator = mqttValidator;
            this.logger = Log.ForContext("Grain", nameof(MqttClientGrain));
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
            this.logger = Log.ForContext("Grain", nameof(MqttClientGrain)).ForContext("Id", this.clientId);
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
                return await this.mqttValidator.ValidateConnection(context, this.userRepository, this.users, PasswordHasher);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
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
                return await this.mqttValidator.ValidatePublish(context, this.userRepository, this.users, DataLimitCacheMonth);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
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
                return await this.mqttValidator.ValidateSubscription(context, this.userRepository, this.users);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        /// Proceeds the disconnection message for one client identifier.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <seealso cref="IMqttClientGrain" />
        public void ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            try
            {
                this.users.Remove(eventArgs.ClientId);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
            }
        }

        /// <inheritdoc cref="IMqttClientGrain" />
        /// <summary>
        /// Checks whether the user is a user used for synchronization.
        /// </summary>
        /// <param name="clientIdParameter">The client identifier.</param>
        /// <returns>A value indicating whether the user is a broker user or not.</returns>
        /// <seealso cref="IMqttClientGrain" />
        public async Task<bool> IsUserBrokerUser(string clientIdParameter)
        {
            try
            {
                return await this.mqttValidator.IsUserBrokerUser(clientIdParameter, this.userRepository, this.users);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
                return false;
            }
        }
    }
}
