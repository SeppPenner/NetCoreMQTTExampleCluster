// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttRepositoryGrain.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
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
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using MQTTnet;
    using MQTTnet.Client.Options;
    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    using Orleans;
    using Orleans.Concurrency;

    using Serilog;

    /// <inheritdoc cref="IMqttRepositoryGrain" />
    [Reentrant]
    public class MqttRepositoryGrain : Grain, IMqttRepositoryGrain
    {
        /// <summary>
        /// The event log repository.
        /// </summary>
        private readonly IEventLogRepository eventLogRepository;

        /// <summary>
        /// The brokers.
        /// </summary>
        private readonly IDictionary<Guid, IBrokerConnectionSettings> brokers = new ConcurrentDictionary<Guid, IBrokerConnectionSettings>();

        /// <summary>
        /// The event log queue.
        /// </summary>
        private readonly ConcurrentQueue<EventLog> eventLogQueue = new ConcurrentQueue<EventLog>();

        /// <summary>
        /// The publish message queue.
        /// </summary>
        private readonly ConcurrentQueue<PublishMessage> publishMessageQueue = new ConcurrentQueue<PublishMessage>();

        /// <summary>
        /// The publish message repository.
        /// </summary>
        private readonly IPublishMessageRepository publishMessageRepository;

        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger logger;

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        public MqttRepositoryGrain(IEventLogRepository eventLogRepository, IPublishMessageRepository publishMessageRepository)
        {
            this.logger = Log.ForContext("Grain", nameof(MqttRepositoryGrain));
            this.eventLogRepository = eventLogRepository;
            this.publishMessageRepository = publishMessageRepository;
        }

        /// <inheritdoc cref="Grain" />
        public override Task OnActivateAsync()
        {
            this.logger = Log.ForContext("Grain", nameof(MqttRepositoryGrain));
            this.RegisterTimer(this.OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20));
            return base.OnActivateAsync();
        }

        /// <inheritdoc cref="Grain" />
        public override async Task OnDeactivateAsync()
        {
            await this.OnTimer(null);
            await base.OnDeactivateAsync();
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        public Task ConnectBroker(IBrokerConnectionSettings brokerConnectionSettings, Guid brokerId)
        {
            if (brokerConnectionSettings is null)
            {
                throw new ArgumentNullException(nameof(brokerConnectionSettings));
            }

            if (brokerId == default)
            {
                throw new ArgumentNullException(nameof(brokerId));
            }

            // Save connect to the database
            var eventLog = new EventLog
            {
                EventType = EventType.BrokerConnect,
                EventDetails = $"New broker connected: BrokerId = {brokerId}."
            };

            this.eventLogQueue.Enqueue(eventLog);

            // Add to dictionary
            this.brokers[brokerId] = brokerConnectionSettings;

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        public Task DisconnectBroker(Guid brokerId)
        {
            if (brokerId == default)
            {
                throw new ArgumentNullException(nameof(brokerId));
            }

            // Save disconnect to the database
            var eventLog = new EventLog
            {
                EventType = EventType.BrokerDisconnect,
                EventDetails = $"Broker disconnected: BrokerId = {brokerId}."
            };

            this.eventLogQueue.Enqueue(eventLog);

            // Remove from broker list
            this.brokers.Remove(brokerId);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
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

                this.eventLogQueue.Enqueue(eventLog);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {Exception}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        public Task ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            if (string.IsNullOrWhiteSpace(eventArgs.ClientId))
            {
                throw new ArgumentNullException(nameof(eventArgs.ClientId));
            }

            // Save disconnect to the database
            var eventLog = new EventLog
            {
                EventType = EventType.Disconnect,
                EventDetails = $"Disconnected: ClientId = {eventArgs.ClientId}, DisconnectType = {eventArgs.DisconnectType}."
            };

            this.eventLogQueue.Enqueue(eventLog);

            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        [AlwaysInterleave]
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
                var payloadString = context.ApplicationMessage?.Payload is null ? string.Empty : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);

                var publishMessage = new PublishMessage
                {
                    ClientId = context.ClientId,
                    Topic = context.ApplicationMessage?.Topic,
                    Payload = new PublishedMessagePayload(payloadString),
                    QoS = context.ApplicationMessage?.QualityOfServiceLevel,
                    Retain = context.ApplicationMessage?.Retain
                };

                this.publishMessageQueue.Enqueue(publishMessage);

                // Publish messages to the broker if the publishing user is not the synchronization user
                var isUserBrokerUser = await mqttClientGrain.IsUserBrokerUser();

                if (!isUserBrokerUser)
                {
                    this.PublishMessageToBrokers(context, brokerId).Ignore();
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {Exception}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
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

                this.eventLogQueue.Enqueue(eventLog);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {Exception}.", ex);
                return false;
            }
        }

        /// <inheritdoc cref="IMqttRepositoryGrain" />
        public Task ProceedUnsubscription(MqttUnsubscriptionInterceptorContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrWhiteSpace(context.ClientId))
            {
                throw new ArgumentNullException(nameof(context.ClientId));
            }

            if (string.IsNullOrWhiteSpace(context.Topic))
            {
                throw new ArgumentNullException(nameof(context.Topic));
            }

            // Save unsubscription to the database
            var eventLog = new EventLog
            {
                EventType = EventType.Unsubscription,
                EventDetails = $"Unsubscription: ClientId = {context.ClientId}, Topic = {context.Topic}."
            };

            this.eventLogQueue.Enqueue(eventLog);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Publishes a message to a remote broker that hasn't initially sent the message to the cluster.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerConnectionSettings">The broker connection settings.</param>
        /// <returns>A <see cref="Task" /> representing asynchronous operation.</returns>
        private static async Task PublishMessageToBroker(MqttApplicationMessageInterceptorContext context, IBrokerConnectionSettings brokerConnectionSettings)
        {
            if (context.ApplicationMessage is null)
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
            var payloadString = context.ApplicationMessage?.Payload is null ? string.Empty : Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

            // Connect the MQTT client
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Send the message
            var message = new MqttApplicationMessageBuilder().WithTopic(context.ApplicationMessage.Topic).WithPayload(payloadString).WithQualityOfServiceLevel(context.ApplicationMessage.QualityOfServiceLevel)
                .WithRetainFlag(context.ApplicationMessage.Retain).Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
            await mqttClient.DisconnectAsync(null, CancellationToken.None);
        }

        /// <summary>
        /// Publishes a message to all remote brokers that haven't initially sent the message to the cluster.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerId">The broker identifier.</param>
        /// <returns>A <see cref="Task" /> representing asynchronous operation.</returns>
        private async Task PublishMessageToBrokers(MqttApplicationMessageInterceptorContext context, Guid brokerId)
        {
            var tasks = this.brokers
                .Where(kvp => kvp.Key != brokerId)
                .Select(b => PublishMessageToBroker(context, b.Value));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Runs the timer function and writes the data to the database.
        /// </summary>
        /// <param name="state">The state object.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        private async Task OnTimer(object state)
        {
            try
            {
                this.logger.Information(
                    "Periodic persisting started, publish message queue size is {PublishMessageQueueCount}, event log queue size is {EventLogQueueCount}.",
                    this.publishMessageQueue.Count,
                    this.eventLogQueue.Count);
                await this.StoreEventLogs();
                await this.StorePublishMessages();
                this.logger.Information("Periodic persisting finished.");
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {Exception}.", ex);
            }
        }

        /// <summary>
        /// Stores the event logs from the queue to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        private async Task StoreEventLogs()
        {
            var eventLogs = new List<EventLog>();

            while (this.eventLogQueue.TryDequeue(out var eventLog))
            {
                eventLogs.Add(eventLog);
            }

            await this.eventLogRepository.InsertEventLogs(eventLogs);
        }

        /// <summary>
        /// Stores the publish messages from the queue to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        private async Task StorePublishMessages()
        {
            var publishMessages = new List<PublishMessage>();

            while (this.publishMessageQueue.TryDequeue(out var publishMessage))
            {
                publishMessages.Add(publishMessage);
            }

            await this.publishMessageRepository.InsertPublishMessages(publishMessages);
        }
    }
}
