// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttService.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The main class that runs the MQTT service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;

    using Microsoft.Extensions.Logging;

    using MQTTnet;
    using MQTTnet.Protocol;
    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Models;

    using Newtonsoft.Json;

    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;

    using Serilog;

    using OrleansMessageRejectionException = Orleans.Runtime.OrleansMessageRejectionException;

    /// <summary>
    ///     The main class that runs the MQTT service.
    /// </summary>
    public class MqttService
    {
        /// <summary>
        ///     The current assembly's executing path.
        /// </summary>
        private readonly string currentPath;

        /// <summary>
        /// The broker identifier.
        /// </summary>
        private readonly Guid brokerId;

        /// <summary>
        ///     The MQTT server.
        /// </summary>
        private IMqttServer mqttServer;

        /// <summary>
        /// The cluster client.
        /// </summary>
        private IClusterClient clusterClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttService" /> class.
        /// </summary>
        /// <param name="currentPath">
        ///     The current assembly's executing path.
        /// </param>
        public MqttService(string currentPath)
        {
            this.currentPath = currentPath;
            this.brokerId = Guid.NewGuid();
        }

        /// <summary>
        ///     Starts the service.
        /// </summary>
        public void Start()
        {
            try
            {
                var certificate = new X509Certificate2(
                    Path.Combine(this.currentPath, "certificate.pfx"),
                    "test",
                    X509KeyStorageFlags.Exportable);

                Config config;

                using (var r = new StreamReader($"{this.currentPath}\\NetCoreMQTTExampleCluster.Cluster.json"))
                {
                    var json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json);
                }

                var optionsBuilder = new MqttServerOptionsBuilder();

                optionsBuilder = config.UnencryptedPort == null
                                     ? optionsBuilder.WithoutDefaultEndpoint()
                                     : optionsBuilder.WithDefaultEndpoint()
                                         .WithDefaultEndpointPort(config.UnencryptedPort.Value);

                optionsBuilder.WithEncryptedEndpoint().WithEncryptedEndpointPort(config.Port)
                    .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                    .WithEncryptionSslProtocol(SslProtocols.Tls12)
                    .WithConnectionValidator(this.ValidateConnection)
                    .WithSubscriptionInterceptor(this.ValidateSubscription)
                    .WithApplicationMessageInterceptor(this.ValidatePublish);

                this.mqttServer = new MqttFactory().CreateMqttServer();
                this.mqttServer.StartAsync(optionsBuilder.Build());
                this.mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(this.HandleUnsubscription);
                this.mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(this.HandleDisconnect);

                this.clusterClient = ConnectOrleansClient(config).Result;
                var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                repositoryGrain.ConnectBroker(config.BrokerConnectionSettings, this.brokerId);

                Log.Information("Started MQTT server.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Stops the service.
        /// </summary>
        public void Stop()
        {
            try
            {
                this.mqttServer.StopAsync();
                var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                repositoryGrain.DisconnectBroker(this.brokerId);
                Log.Information("Stopped MQTT server.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
            }
        }

        /// <summary>
        /// Connects the cluster client to the cluster.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>A <see cref="IClusterClient"/>.</returns>
        private static async Task<IClusterClient> ConnectOrleansClient(Config config)
        {
            Log.Information("Connecting to Orleans cluster.");

            var repeatCount = 0;

            while (true)
            {
                try
                {
                    var client = new ClientBuilder()
                       .Configure<ClusterOptions>(options =>
                       {
                           var clusterOptions = config.OrleansConfiguration.ClusterOptions;
                           options.ClusterId = clusterOptions.ClusterId;
                           options.ServiceId = clusterOptions.ServiceId;
                       })
                       .UseAdoNetClustering(options =>
                       {
                           options.Invariant = "Npgsql";
                           options.ConnectionString = config.DatabaseSettings.ToConnectionString();
                       })
                       .AddSimpleMessageStreamProvider("SMSProvider")
                       .ConfigureLogging(logging =>
                       {
                           logging.ClearProviders();
                           logging.AddSerilog(dispose: true, logger: Log.Logger);
                       })
                       .Build();

                    await client.Connect();
                    Log.Information("Client successfully connected to silo host.");
                    return client;
                }
                catch (OrleansMessageRejectionException ex)
                {
                    Log.Error($"Connect failed: {ex.Message}.");
                }

                repeatCount++;
                if (repeatCount > 50)
                {
                    await Task.Delay(16000);
                }
                else if (repeatCount > 30)
                {
                    await Task.Delay(4000);
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        ///     Handles the MQTT unsubscription event.
        /// </summary>
        /// <param name="eventArgs">The MQTT client unsubscribed event args.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void HandleUnsubscription(MqttServerClientUnsubscribedTopicEventArgs eventArgs)
        {
            var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
            repositoryGrain.ProceedUnsubscription(eventArgs);
        }

        /// <summary>
        ///     Handles the disconnect event.
        /// </summary>
        /// <param name="eventArgs">The MQTT client unsubscribed event args.</param>
        private void HandleDisconnect(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
            repositoryGrain.ProceedDisconnect(eventArgs);
        }

        /// <summary>
        ///     Validates the connection.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ValidateConnection(MqttConnectionValidatorContext context)
        {
            var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
            var connectionValid = repositoryGrain.ProceedConnect(new SimpleMqttConnectionValidatorContext(context)).Result;
            context.ReasonCode = connectionValid ? MqttConnectReasonCode.Success : MqttConnectReasonCode.BadUserNameOrPassword;
        }

        /// <summary>
        ///     Validates the message publication.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ValidatePublish(MqttApplicationMessageInterceptorContext context)
        {
            var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
            var publishValid = repositoryGrain.ProceedPublish(context, this.brokerId).Result;
            context.AcceptPublish = publishValid;
        }

        /// <summary>
        ///     Validates the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ValidateSubscription(MqttSubscriptionInterceptorContext context)
        {
            var repositoryGrain = this.clusterClient.GetGrain<IMqttRepositoryGrain>(0);
            var subscriptionValid = repositoryGrain.ProceedSubscription(context).Result;
            context.AcceptSubscription = subscriptionValid;
        }
    }
}