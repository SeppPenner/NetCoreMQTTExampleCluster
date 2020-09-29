// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttService.cs" company="Hämmer Electronics">
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
    using System.Reflection;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;

    using MQTTnet;
    using MQTTnet.Protocol;
    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Cluster.Configuration;
    using NetCoreMQTTExampleCluster.Grains.Interfaces;

    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;
    using Orleans.Runtime;

    using Polly;

    using Serilog;

    using ILogger = Serilog.ILogger;

    /// <inheritdoc cref="BackgroundService"/>
    /// <inheritdoc cref="IMqttServerSubscriptionInterceptor"/>
    /// <inheritdoc cref="IMqttServerUnsubscriptionInterceptor"/>
    /// <inheritdoc cref="IMqttServerApplicationMessageInterceptor"/>
    /// <inheritdoc cref="IMqttServerConnectionValidator"/>
    /// <inheritdoc cref="IMqttServerClientDisconnectedHandler"/>
    /// <summary>
    ///     The main class that runs the MQTT service.
    /// </summary>
    /// <seealso cref="BackgroundService"/>
    /// <seealso cref="IMqttServerSubscriptionInterceptor"/>
    /// <seealso cref="IMqttServerUnsubscriptionInterceptor"/>
    /// <seealso cref="IMqttServerApplicationMessageInterceptor"/>
    /// <seealso cref="IMqttServerConnectionValidator"/>
    /// <seealso cref="IMqttServerClientDisconnectedHandler"/>
    public class MqttService : BackgroundService, IMqttServerSubscriptionInterceptor, IMqttServerUnsubscriptionInterceptor, IMqttServerApplicationMessageInterceptor, IMqttServerConnectionValidator, IMqttServerClientDisconnectedHandler
    {
        /// <summary>
        /// The cluster client.
        /// </summary>
        private static IClusterClient clusterClient;

        /// <summary>
        ///     The cluster configuration.
        /// </summary>
        private readonly ClusterConfiguration clusterConfiguration;

        /// <summary>
        /// The broker identifier.
        /// </summary>
        private readonly Guid brokerId;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger = Log.ForContext<MqttService>();

        /// <summary>
        ///     The MQTT server.
        /// </summary>
        private IMqttServer mqttServer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttService" /> class.
        /// </summary>
        /// <param name="clusterConfiguration">
        ///     The cluster configuration.
        /// </param>
        public MqttService(ClusterConfiguration clusterConfiguration)
        {
            this.brokerId = Guid.NewGuid();
            this.clusterConfiguration = clusterConfiguration;
        }

        /// <inheritdoc cref="BackgroundService"/>
        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <seealso cref="BackgroundService"/>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.Information("Starting MQTT server...");

            try
            {
                var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                var certificate = new X509Certificate2(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Path.Combine(currentLocation, "certificate.pfx"),
                    "test",
                    X509KeyStorageFlags.Exportable);

                var optionsBuilder = new MqttServerOptionsBuilder();

                optionsBuilder = this.clusterConfiguration.UnencryptedPort == null
                                     ? optionsBuilder.WithoutDefaultEndpoint()
                                     : optionsBuilder.WithDefaultEndpoint()
                                         .WithDefaultEndpointPort(this.clusterConfiguration.UnencryptedPort.Value);

                optionsBuilder.WithEncryptedEndpoint().WithEncryptedEndpointPort(this.clusterConfiguration.Port)
                    .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                    .WithEncryptionSslProtocol(SslProtocols.Tls12)
                    .WithConnectionValidator(this)
                    .WithSubscriptionInterceptor(this)
                    .WithApplicationMessageInterceptor(this)
                    .WithUnsubscriptionInterceptor(this);

                this.mqttServer = new MqttFactory().CreateMqttServer();

                // Todo: Move this to handler once available
                this.mqttServer.ClientDisconnectedHandler = this;

                await this.mqttServer.StartAsync(optionsBuilder.Build());

                await this.ConnectOrleansClient();
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                await repositoryGrain.ConnectBroker(this.clusterConfiguration.BrokerConnectionSettings, this.brokerId);

                this.logger.Information("Started MQTT server.");
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                this.logger.Fatal("An error occurred: {@ex}.", ex);
            }

            await base.StartAsync(cancellationToken);
        }

        /// <inheritdoc cref="BackgroundService"/>
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <seealso cref="BackgroundService"/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.Information("Stopping MQTT server...");

            try
            {
                await this.mqttServer.StopAsync();
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                await repositoryGrain.DisconnectBroker(this.brokerId);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                this.logger.Fatal("An error occurred: {@ex}.", ex);
            }
        }

        /// <summary>
        /// Handles the MQTT unsubscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public async Task InterceptUnsubscriptionAsync(MqttUnsubscriptionInterceptorContext context)
        {
            try
            {
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                await repositoryGrain.ProceedUnsubscription(context);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {@ex}.", ex);
            }
        }

        /// <summary>
        /// Validates the MQTT connection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        public async Task ValidateConnectionAsync(MqttConnectionValidatorContext context)
        {
            try
            {
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                var connectionValid = await repositoryGrain.ProceedConnect(new SimpleMqttConnectionValidatorContext(context));
                context.ReasonCode = connectionValid ? MqttConnectReasonCode.Success : MqttConnectReasonCode.BadUserNameOrPassword;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {@ex}.", ex);
            }
        }

        /// <summary>
        /// Validates the MQTT subscriptions.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        public async Task InterceptSubscriptionAsync(MqttSubscriptionInterceptorContext context)
        {
            try
            {
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                var subscriptionValid = await repositoryGrain.ProceedSubscription(context);
                context.AcceptSubscription = subscriptionValid;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {@ex}.", ex);
            }
        }

        /// <summary>
        /// Validates the MQTT application messages.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        public async Task InterceptApplicationMessagePublishAsync(MqttApplicationMessageInterceptorContext context)
        {
            try
            {
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                var publishValid = await repositoryGrain.ProceedPublish(context, this.brokerId);
                context.AcceptPublish = publishValid;
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {@ex}.", ex);
            }
        }

        /// <summary>
        /// Validates the MQTT client disconnect.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        public async Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            try
            {
                var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(0);
                await repositoryGrain.ProceedDisconnect(eventArgs);
            }
            catch (Exception ex)
            {
                this.logger.Error("An error occurred: {@ex}.", ex);
            }
        }

        /// <inheritdoc cref="BackgroundService"/>
        /// <summary>
        /// This method is called when the <see cref="BackgroundService"/> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="BackgroundService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        /// <seealso cref="BackgroundService"/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.logger.Information("Heartbeat.");
                await Task.Delay(this.clusterConfiguration.HeartbeatIntervalInMilliseconds, stoppingToken);
            }
        }

        /// <summary>
        /// Connects the cluster client to the cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        private async Task ConnectOrleansClient()
        {
            this.logger.Information("Connecting to Orleans cluster.");

            await Policy.Handle<Exception>().WaitAndRetryForeverAsync(
                i => TimeSpan.FromSeconds(3),
                (ex, ts) =>
                {
                    if (ex.GetType() == typeof(SiloUnavailableException))
                    {
                        this.logger.Error("Silo is not available...");
                    }
                    else
                    {
                        this.logger.Error(ex, "Connection to Orleans cluster failed!");
                        clusterClient?.Dispose();
                    }
                }).ExecuteAsync(
                async () =>
                {
                    // TODO: check if db connection is needed here or static clustering is the better approach to connect to silo host
                    clusterClient = new ClientBuilder().Configure<ClusterOptions>(
                        options =>
                        {
                            var clusterOptions = this.clusterConfiguration.OrleansConfiguration.ClusterOptions;
                            options.ClusterId = clusterOptions.ClusterId;
                            options.ServiceId = clusterOptions.ServiceId;
                        }).UseAdoNetClustering(
                        options =>
                        {
                            options.Invariant = "Npgsql";
                            options.ConnectionString = this.clusterConfiguration.DatabaseSettings.ToConnectionString();
                        }).AddSimpleMessageStreamProvider("SMSProvider").ConfigureLogging(
                        logging =>
                        {
                            logging.AddSerilog();
                        }).Build();
                    await clusterClient.Connect(
                        async ex =>
                        {
                            await Task.Delay(500);
                            return true;
                        });

                    this.logger.Information("Connection to Orleans cluster successful!");
                });

            while (!clusterClient.IsInitialized)
            {
                await Task.Delay(500);
            }
        }
    }
}