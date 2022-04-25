// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main class that runs the MQTT service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NetCoreMQTTExampleCluster.Cluster;

/// <inheritdoc cref="BackgroundService"/>
/// <inheritdoc cref="IMqttServerSubscriptionInterceptor"/>
/// <inheritdoc cref="IMqttServerUnsubscriptionInterceptor"/>
/// <inheritdoc cref="IMqttServerApplicationMessageInterceptor"/>
/// <inheritdoc cref="IMqttServerConnectionValidator"/>
/// <inheritdoc cref="IMqttServerClientDisconnectedHandler"/>
/// <summary>
/// The main class that runs the MQTT service.
/// </summary>
public class MqttService : BackgroundService, IMqttServerSubscriptionInterceptor, IMqttServerUnsubscriptionInterceptor, IMqttServerApplicationMessageInterceptor, IMqttServerConnectionValidator, IMqttServerClientDisconnectedHandler
{
    /// <summary>
    /// The cluster client.
    /// </summary>
    private static IClusterClient clusterClient;

    /// <summary>
    /// The cluster configuration.
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
    /// The MQTT server.
    /// </summary>
    private IMqttServer mqttServer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttService" /> class.
    /// </summary>
    /// <param name="clusterConfiguration">
    /// The cluster configuration.
    /// </param>
    public MqttService(ClusterConfiguration clusterConfiguration)
    {
        this.brokerId = Guid.NewGuid();
        this.clusterConfiguration = clusterConfiguration;
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.Information("Starting MQTT server...");

        try
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var certificate = new X509Certificate2(
                Path.Combine(currentLocation, "certificate.pfx"),
                "test",
                X509KeyStorageFlags.Exportable);

            var optionsBuilder = new MqttServerOptionsBuilder();

            optionsBuilder = this.clusterConfiguration.UnencryptedPort is null
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
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            await repositoryGrain.ConnectBroker(this.clusterConfiguration.BrokerConnectionSettings, this.brokerId);

            this.logger.Information("Started MQTT server.");
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            this.logger.Fatal("An error occurred: {Exception}.", ex);
        }

        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.Information("Stopping MQTT server...");

        try
        {
            await this.mqttServer.StopAsync();
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            await repositoryGrain.DisconnectBroker(this.brokerId);
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            this.logger.Fatal("An error occurred: {Exception}.", ex);
        }
    }

    /// <inheritdoc cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.Information("Heartbeat.");
            await Task.Delay(this.clusterConfiguration.HeartbeatIntervalInMilliseconds, stoppingToken);
        }
    }

    /// <summary>
    /// Handles the MQTT unsubscription.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task InterceptUnsubscriptionAsync(MqttUnsubscriptionInterceptorContext context)
    {
        try
        {
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            await repositoryGrain.ProceedUnsubscription(context);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
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
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            var connectionValid = await repositoryGrain.ProceedConnect(new SimpleMqttConnectionValidatorContext(context));
            context.ReasonCode = connectionValid ? MqttConnectReasonCode.Success : MqttConnectReasonCode.BadUserNameOrPassword;
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
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
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            var subscriptionValid = await repositoryGrain.ProceedSubscription(context);
            context.AcceptSubscription = subscriptionValid;
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
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
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            var publishValid = await repositoryGrain.ProceedPublish(context, this.brokerId);
            context.AcceptPublish = publishValid;
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
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
            var repositoryGrain = clusterClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            await repositoryGrain.ProceedDisconnect(eventArgs);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
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
                clusterClient = new ClientBuilder().Configure<ClusterOptions>(
                    options =>
                    {
                        var clusterOptions = this.clusterConfiguration.OrleansConfiguration.ClusterOptions;
                        options.ClusterId = clusterOptions.ClusterId;
                        options.ServiceId = clusterOptions.ServiceId;
                    }).UseAdoNetClustering(
                    options =>
                    {
                        options.Invariant = GlobalConstants.Invariant;
                        options.ConnectionString = this.clusterConfiguration.DatabaseSettings.ToConnectionString();
                    }).AddSimpleMessageStreamProvider(GlobalConstants.SimpleMessageStreamProvider).ConfigureLogging(
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
