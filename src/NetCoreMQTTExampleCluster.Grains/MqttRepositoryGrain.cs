// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttRepositoryGrain.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The grain for a repository to manage the brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    using MQTTnet;
    using MQTTnet.Client.Options;
    using MQTTnet.Server;

    using Orleans;

    using Serilog;

    /// <inheritdoc cref="IMqttRepositoryGrain" />
    /// <summary>
    ///     The grain for a repository to manage the brokers.
    /// </summary>
    /// <seealso cref="IMqttRepositoryGrain" />
    public class MqttRepositoryGrain : Grain, IMqttRepositoryGrain
    {
        /// <summary>
        ///     The event log repository.
        /// </summary>
        private readonly IEventLogRepository eventLogRepository;

        /// <summary>
        ///     The brokers.
        /// </summary>
        private readonly IDictionary<Guid, IBrokerConnectionSettings> brokers = new ConcurrentDictionary<Guid, IBrokerConnectionSettings>();

        /// <summary>
        ///     The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     The publish message repository.
        /// </summary>
        private readonly IPublishMessageRepository publishMessageRepository;

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttRepositoryGrain" /> class.
        /// </summary>
        /// <param name="eventLogRepository">The event log repository.</param>
        /// <param name="publishMessageRepository">The publish message repository.</param>
        /// <seealso cref="IMqttRepositoryGrain" />
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public MqttRepositoryGrain(IEventLogRepository eventLogRepository, IPublishMessageRepository publishMessageRepository)
        {
            this.logger = Log.ForContext("Grain", nameof(MqttRepositoryGrain));
            this.eventLogRepository = eventLogRepository;
            this.publishMessageRepository = publishMessageRepository;
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Connects a broker to the grain.
        /// </summary>
        /// <param name="brokerConnectionSettings">The broker connection settings.</param>
        /// <param name="brokerId">The broker identifier.</param>
        /// <seealso cref="IMqttRepositoryGrain" />
        public async void ConnectBroker(IBrokerConnectionSettings brokerConnectionSettings, Guid brokerId)
        {
            try
            {
                // Save connect to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.BrokerConnect,
                    EventDetails = $"New broker connected: BrokerId = {brokerId}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);

                // Add to dictionary
                this.brokers[brokerId] = brokerConnectionSettings;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Disconnects the broker from the grain.
        /// </summary>
        /// <param name="brokerId">The broker identifier.</param>
        /// <seealso cref="IMqttRepositoryGrain" />
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public async void DisconnectBroker(Guid brokerId)
        {
            try
            {
                // Save disconnect to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.BrokerDisconnect,
                    EventDetails = $"Broker disconnected: BrokerId = {brokerId}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);

                // Remove from broker list
                this.brokers.Remove(brokerId);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Proceeds the connection for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        /// <seealso cref="IMqttRepositoryGrain" />
        public async Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context)
        {
            try
            {
                // Handle connect in grain
                var mqttClientGrain = this.GrainFactory.GetGrain<IMqttClientGrain>(context.ClientId);
                var connectValid = await mqttClientGrain.ProceedConnect(context);

                if (!connectValid)
                {
                    return false;
                }

                // Save connect to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.Connect,
                    EventDetails = $"New connection: ClientId = {context.ClientId}, Endpoint = {context.Endpoint}," + $" Username = {context.UserName}, Password = {context.Password}," + $" CleanSession = {context.CleanSession}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Proceeds the disconnection for one client identifier.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <returns>A <see cref="Task" /> returning any asynchronous operation.</returns>
        /// <seealso cref="IMqttRepositoryGrain" />
        public async Task ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            try
            {
                if (eventArgs == null)
                {
                    return;
                }

                // Save disconnect to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.Disconnect,
                    EventDetails = $"Disconnected: ClientId = {eventArgs.ClientId}, DisconnectType = {eventArgs.DisconnectType}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);

                // Handle disconnect in grain
                var mqttClientGrain = this.GrainFactory.GetGrain<IMqttClientGrain>(eventArgs.ClientId);
                mqttClientGrain.ProceedDisconnect(eventArgs);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Proceeds the published message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerId">The broker identifier.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        /// <seealso cref="IMqttRepositoryGrain" />
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public async Task<bool> ProceedPublish(MqttApplicationMessageInterceptorContext context, Guid brokerId)
        {
            try
            {
                // Handle published message in grains
                var mqttClientGrain = this.GrainFactory.GetGrain<IMqttClientGrain>(context.ClientId);
                var publishValid = await mqttClientGrain.ProceedPublish(context);

                if (!publishValid)
                {
                    return false;
                }

                // Save published message to the database
                var payloadString = context.ApplicationMessage?.Payload == null ? string.Empty : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);

                var publishMessage = new PublishMessage
                {
                    ClientId = context.ClientId,
                    Topic = context.ApplicationMessage?.Topic,
                    Payload = new PublishedMessagePayload(payloadString),
                    QoS = context.ApplicationMessage?.QualityOfServiceLevel,
                    Retain = context.ApplicationMessage?.Retain
                };

                await this.publishMessageRepository.InsertPublishMessage(publishMessage);

                // Publish messages to the broker if the publishing user is not the synchronization user
                var isUserBrokerUser = await mqttClientGrain.IsUserBrokerUser(context.ClientId);

                if (!isUserBrokerUser)
                {
                    // ReSharper disable once AssignmentIsFullyDiscarded
                    _ = this.PublishMessageToBrokers(context, brokerId);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Proceeds the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        /// <seealso cref="IMqttRepositoryGrain" />
        public async Task<bool> ProceedSubscription(MqttSubscriptionInterceptorContext context)
        {
            try
            {
                // Handle subscription in grain
                var mqttClientGrain = this.GrainFactory.GetGrain<IMqttClientGrain>(context.ClientId);
                var subscriptionValid = await mqttClientGrain.ProceedSubscription(context);

                if (!subscriptionValid)
                {
                    return false;
                }

                // Save subscription to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.Subscription,
                    EventDetails = $"New subscription: ClientId = {context.ClientId}, TopicFilter = {context.TopicFilter}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        /// <summary>
        ///     Proceeds the unsubscription for one client identifier.
        /// </summary>
        /// <param name="eventArgs">
        ///     The event args.
        /// </param>
        /// <returns>A <see cref="Task" /> returning any asynchronous operation.</returns>
        /// <seealso cref="IMqttRepositoryGrain" />
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public async Task ProceedUnsubscription(MqttServerClientUnsubscribedTopicEventArgs eventArgs)
        {
            try
            {
                if (eventArgs == null)
                {
                    return;
                }

                // Save unsubscription to the database
                var eventLog = new EventLog
                {
                    EventType = EventType.Unsubscription,
                    EventDetails = $"Unsubscription: ClientId = {eventArgs.ClientId}, TopicFilter = {eventArgs.TopicFilter}."
                };

                await this.eventLogRepository.InsertEventLog(eventLog);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occured: {ex}.", ex);
            }
        }

        /// <summary>
        ///     Publishes a message to a remote broker that hasn't initially sent the message to the cluster.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerConnectionSettings">The broker connection settings.</param>
        /// <returns>A <see cref="Task" /> representing asynchronous operation.</returns>
        private static async Task PublishMessageToBroker(MqttApplicationMessageInterceptorContext context, IBrokerConnectionSettings brokerConnectionSettings)
        {
            if (context.ApplicationMessage == null)
            {
                return;
            }

            // Create a new MQTT client
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var optionsBuilder = new MqttClientOptionsBuilder().WithClientId(brokerConnectionSettings.ClientId).WithTcpServer(brokerConnectionSettings.HostName, brokerConnectionSettings.Port)
                .WithCredentials(brokerConnectionSettings.UserName, brokerConnectionSettings.Password).WithCleanSession(brokerConnectionSettings.UseCleanSession);

            if (brokerConnectionSettings.UseTls)
            {
                optionsBuilder.WithTls();
            }

            var options = optionsBuilder.Build();

            // Deserialize payload
            var payloadString = context.ApplicationMessage?.Payload == null ? string.Empty : Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

            // Connect the MQTT client
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Send the message
            var message = new MqttApplicationMessageBuilder().WithTopic(context.ApplicationMessage.Topic).WithPayload(payloadString).WithQualityOfServiceLevel(context.ApplicationMessage.QualityOfServiceLevel)
                .WithRetainFlag(context.ApplicationMessage.Retain).Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
            await mqttClient.DisconnectAsync(null, CancellationToken.None);
        }

        /// <summary>
        ///     Publishes a message to all remote brokers that haven't initially sent the message to the cluster.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerId">The broker identifier.</param>
        /// <returns>A <see cref="Task" /> representing asynchronous operation.</returns>
        private async Task PublishMessageToBrokers(MqttApplicationMessageInterceptorContext context, Guid brokerId)
        {
            foreach (var (key, settings) in this.brokers)
            {
                if (key == brokerId)
                {
                    continue;
                }

                await PublishMessageToBroker(context, settings);
            }
        }
    }
}
