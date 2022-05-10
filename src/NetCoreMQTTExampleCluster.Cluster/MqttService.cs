// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main class that runs the MQTT service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NetCoreMQTTExampleCluster.Cluster;

/// <inheritdoc cref="BackgroundServiceBase{T}"/>
/// <inheritdoc cref="IMqttServerSubscriptionInterceptor"/>
/// <inheritdoc cref="IMqttServerUnsubscriptionInterceptor"/>
/// <inheritdoc cref="IMqttServerApplicationMessageInterceptor"/>
/// <inheritdoc cref="IMqttServerConnectionValidator"/>
/// <inheritdoc cref="IMqttServerClientDisconnectedHandler"/>
/// <summary>
/// The main class that runs the MQTT service.
/// </summary>
public class MqttService : BackgroundServiceBase<ClusterConfiguration>, IMqttServerSubscriptionInterceptor, IMqttServerUnsubscriptionInterceptor,
    IMqttServerApplicationMessageInterceptor, IMqttServerConnectionValidator, IMqttServerClientDisconnectedHandler
{
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
    [NotNull]
    private IMqttServer? mqttServer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttService" /> class.
    /// </summary>
    /// <param name="clusterConfiguration">
    /// The cluster configuration.
    /// </param>
    public MqttService(ClusterConfiguration clusterConfiguration) : base(clusterConfiguration)
    {
        this.brokerId = Guid.NewGuid();
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token, cancellationToken);

        // Connect Orleans.
        await this.ConnectOrleans(() => this.CreateOrleansClient(), linkedTokenSource.Token);

        // Startup the service.
        await this.StartupService();
        await base.StartAsync(linkedTokenSource.Token);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Stopping MQTT service...");
        await this.StopService("MQTT service is stopped.", cancellationToken);
        await this.mqttServer.StopAsync();
        var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
        await repositoryGrain.DisconnectBroker(this.brokerId);
        await base.StopAsync(cancellationToken);
#pragma warning disable CA1816 // Dispose-Methoden müssen SuppressFinalize aufrufen
        GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose-Methoden müssen SuppressFinalize aufrufen
    }

    /// <inheritdoc cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token, cancellationToken);

        while (!linkedTokenSource.IsCancellationRequested)
        {
            // Runs the main task of the service.
            this.TryRunServiceTask();

            // Wait for the next run.
            await Task.Delay(this.ServiceConfiguration.ServiceDelayInMilliSeconds, linkedTokenSource.Token);
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
            var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
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
            var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
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
            var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            var subscriptionValid = await repositoryGrain.ProceedSubscription(new SimpleMqttSubscriptionInterceptorContext(context));
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
            var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            var publishValid = await repositoryGrain.ProceedPublish(new SimpleMqttApplicationMessageInterceptorContext(context), this.brokerId);
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
            var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
            await repositoryGrain.ProceedDisconnect(eventArgs);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
        }
    }

    /// <summary>
    /// Runs the main task of the service.
    /// </summary>
    private void TryRunServiceTask()
    {
        try
        {
            // Run the cyclic MQTT task.
            this.Logger.Information("Started cyclic MQTT task: {Timestamp}", DateTimeOffset.Now);

            // Run the heartbeat and log some memory information.
            this.LogMemoryInformation(this.ServiceConfiguration.HeartbeatIntervalInMilliSeconds, Program.ServiceName.Name ?? "MqttService");

            // All tasks are finished, cyclic MQTT task is done.
            this.Logger.Information("Finished cyclic MQTT task: {Timestamp}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            this.Logger.Error("An error occurred: {Exception}", ex);
        }
    }

    /// <summary>
    /// Creates the Orleans client.
    /// </summary>
    private Task CreateOrleansClient()
    {
        this.OrleansClient = new ClientBuilder()
            .Configure<ClusterOptions>(
                options =>
                {
                    options.ClusterId = this.ServiceConfiguration.OrleansConfiguration!.ClusterOptions!.ClusterId;
                    options.ServiceId = this.ServiceConfiguration.OrleansConfiguration!.ClusterOptions!.ServiceId;
                })
            .UseAdoNetClustering(
                options =>
                {
                    options.Invariant = GlobalConstants.Invariant;
                    options.ConnectionString = this.ServiceConfiguration.DatabaseSettings!.ToConnectionString();
                })
            .ConfigureApplicationParts(
                parts => parts.AddApplicationPart(typeof(IMqttRepositoryGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddSerilog())
            .AddSimpleMessageStreamProvider(GlobalConstants.SimpleMessageStreamProvider)
            .Build();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts up the service.
    /// </summary>
    private async Task StartupService()
    {
        this.Logger.Information("Starting MQTT service...");

        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (string.IsNullOrWhiteSpace(currentLocation))
        {
            throw new InvalidOperationException("The current location is empty.");
        }

        // Todo: Move to configuration.
        var certificate = new X509Certificate2(
            Path.Combine(currentLocation, "certificate.pfx"),
            "test",
            X509KeyStorageFlags.Exportable);

        var optionsBuilder = new MqttServerOptionsBuilder();

        optionsBuilder = this.ServiceConfiguration.UnencryptedPort is null
                             ? optionsBuilder.WithoutDefaultEndpoint()
                             : optionsBuilder.WithDefaultEndpoint()
                                 .WithDefaultEndpointPort(this.ServiceConfiguration.UnencryptedPort.Value);

        optionsBuilder.WithEncryptedEndpoint().WithEncryptedEndpointPort(this.ServiceConfiguration.Port)
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

        var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
        await repositoryGrain.ConnectBroker(this.ServiceConfiguration.BrokerConnectionSettings!, this.brokerId);

        this.logger.Information("Started MQTT service.");
    }
}
