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
/// <summary>
/// The main class that runs the MQTT service.
/// </summary>
public class MqttService : BackgroundServiceBase<ClusterConfiguration>
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
    private MqttServer? mqttServer = null;

    /// <summary>
    /// The MQTT factory.
    /// </summary>
    private readonly MqttServerFactory mqttServerFactory = new();

    /// <summary>
    /// The client identifiers of the connections of synchronization users. Every accepted connect with the same client
    /// identifier sets or removes the entry, a disconnect removes it.
    /// </summary>
    private readonly ConcurrentDictionary<string, bool> syncClientIds = new();

    /// <summary>
    /// The synchronizer that forwards the messages to the other brokers.
    /// </summary>
    [NotNull]
    private BrokerSynchronizer? brokerSynchronizer = null;

    /// <summary>
    /// The Orleans reference to the <see cref="brokerSynchronizer"/> as observer of the repository grain.
    /// </summary>
    [NotNull]
    private IMqttBrokerObserver? brokerObserver = null;

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
        await this.ConnectOrleans(this.CreateOrleansClientHost, linkedTokenSource.Token);

        // Create the synchronizer, the repository grain tells it about new and removed brokers.
        this.brokerSynchronizer = new BrokerSynchronizer(this.brokerId, this.ServiceConfiguration.BrokerConnectionSettings!.ClientId);
        this.brokerObserver = this.OrleansClient.CreateObjectReference<IMqttBrokerObserver>(this.brokerSynchronizer);

        // Startup the service.
        await this.StartupService();
        await base.StartAsync(linkedTokenSource.Token);
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Stopping MQTT service...");

        // Stop the cyclic task first, so that it doesn't register the broker again after the disconnect.
        this.CancellationTokenSource.Cancel();

        if (this.ExecuteTask is not null)
        {
            try
            {
                await this.ExecuteTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        await this.mqttServer.StopAsync();
        var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
        await repositoryGrain.DisconnectBroker(this.brokerId);
        await this.brokerSynchronizer.DisposeAsync();
        this.OrleansClient.DeleteObjectReference<IMqttBrokerObserver>(this.brokerObserver);
        await this.StopService("MQTT service is stopped.", cancellationToken);
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

            // Registers the broker again, see the method for details.
            await this.RegisterBroker();

            // Wait for the next run.
            await Task.Delay(this.ServiceConfiguration.ServiceDelayInMilliSeconds, linkedTokenSource.Token);
        }
    }

    /// <summary>
    /// Handles the MQTT unsubscription.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task ClientUnsubscribed(ClientUnsubscribedTopicEventArgs args)
    {
        try
        {
            var clientGrain = this.OrleansClient.GetGrain<IMqttClientGrain>(args.ClientId);
            await clientGrain.ProceedUnsubscription(new SimpleClientUnsubscribedTopicEventArgs(args));
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
        }
    }

    /// <summary>
    /// Validates the MQTT connection.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task ValidateConnection(ValidatingConnectionEventArgs args)
    {
        try
        {
            var clientGrain = this.OrleansClient.GetGrain<IMqttClientGrain>(args.ClientId);
            var connectionValid = await clientGrain.ProceedConnect(new SimpleValidatingConnectionEventArgs(args));

            if (!connectionValid)
            {
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                return;
            }

            // Remember the connections of synchronization users, their messages come from another broker.
            if (await clientGrain.IsUserBrokerUser())
            {
                this.syncClientIds[args.ClientId] = true;
            }
            else
            {
                this.syncClientIds.TryRemove(args.ClientId, out _);
            }

            args.ReasonCode = MqttConnectReasonCode.Success;
        }
        catch (Exception ex)
        {
            // Without the silo, the connection can't be validated, so it is refused.
            this.logger.Error("An error occurred: {Exception}.", ex);
            args.ReasonCode = MqttConnectReasonCode.ServerUnavailable;
        }
    }

    /// <summary>
    /// Validates the MQTT subscriptions.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task InterceptSubscription(InterceptingSubscriptionEventArgs args)
    {
        try
        {
            var clientGrain = this.OrleansClient.GetGrain<IMqttClientGrain>(args.ClientId);
            var subscriptionValid = await clientGrain.ProceedSubscription(new SimpleInterceptingSubscriptionEventArgs(args));
            args.ProcessSubscription = subscriptionValid;

            if (!subscriptionValid)
            {
                // Without this, the client gets a SUBACK with a granted QoS although the subscription was rejected.
                args.Response.ReasonCode = MqttSubscribeReasonCode.NotAuthorized;
            }
        }
        catch (Exception ex)
        {
            // Without the silo, the subscription can't be validated, so it is rejected.
            this.logger.Error("An error occurred: {Exception}.", ex);
            args.ProcessSubscription = false;
            args.Response.ReasonCode = MqttSubscribeReasonCode.UnspecifiedError;
        }
    }

    /// <summary>
    /// Validates the MQTT application messages.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task InterceptPublish(InterceptingPublishEventArgs args)
    {
        // A message of a synchronization user was forwarded by another broker, which validated and stored it already.
        // It is only delivered here and not forwarded again, which also prevents loops.
        if (this.syncClientIds.ContainsKey(args.ClientId))
        {
            return;
        }

        try
        {
            var clientGrain = this.OrleansClient.GetGrain<IMqttClientGrain>(args.ClientId);
            var publishValid = await clientGrain.ProceedPublish(new SimpleInterceptingPublishEventArgs(args));
            args.ProcessPublish = publishValid;

            if (!publishValid)
            {
                // Without this, MQTT 5 clients get a PUBACK with success although the message was rejected.
                // MQTT 3.1.1 has no reason code in the PUBACK, so these clients still get a positive acknowledgement.
                args.Response.ReasonCode = MqttPubAckReasonCode.NotAuthorized;
                return;
            }

            this.brokerSynchronizer.Forward(args.ApplicationMessage);
        }
        catch (Exception ex)
        {
            // Without the silo, the message can't be validated, so it is rejected.
            this.logger.Error("An error occurred: {Exception}.", ex);
            args.ProcessPublish = false;
            args.Response.ReasonCode = MqttPubAckReasonCode.UnspecifiedError;
        }
    }

    /// <summary>
    /// Validates the MQTT client disconnect.
    /// </summary>
    /// <param name="args">The event args.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public async Task ClientDisconnected(ClientDisconnectedEventArgs args)
    {
        // On a takeover, the new connection with the same client identifier was validated already and keeps its entry.
        if (args.DisconnectType != MqttClientDisconnectType.Takeover)
        {
            this.syncClientIds.TryRemove(args.ClientId, out _);
        }

        try
        {
            var clientGrain = this.OrleansClient.GetGrain<IMqttClientGrain>(args.ClientId);
            await clientGrain.ProceedDisconnect(new SimpleClientDisconnectedEventArgs(args));
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
        }
    }

    /// <summary>
    /// Registers the broker at the repository grain and updates the forwarding to the other brokers. The grain keeps the brokers
    /// in memory only and loses them if the silo restarts or the grain is deactivated (e.g. after 15 minutes without calls), so
    /// the registration is repeated cyclically.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    private async Task RegisterBroker()
    {
        try
        {
            await this.ConnectBroker();
        }
        catch (Exception ex)
        {
            this.logger.Warning("Registering the broker failed: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Connects the broker to the repository grain and updates the forwarding with the broker list from the grain.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    private async Task ConnectBroker()
    {
        var repositoryGrain = this.OrleansClient.GetGrain<IMqttRepositoryGrain>(GlobalConstants.RepositoryGrainId);
        var brokers = await repositoryGrain.ConnectBroker(this.ServiceConfiguration.BrokerConnectionSettings!, this.brokerId, this.brokerObserver);
        this.brokerSynchronizer.UpdateBrokers(brokers);
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
    /// Creates the host that runs the Orleans client.
    /// </summary>
    /// <returns>The host that runs the Orleans client.</returns>
    private IHost CreateOrleansClientHost()
    {
        const int MaximumConnectionAttempts = 10;
        var connectionAttempts = 0;

        return new HostBuilder()
            .UseOrleansClient(
                client =>
                {
                    client.Configure<ClusterOptions>(
                        options =>
                        {
                            options.ClusterId = this.ServiceConfiguration.OrleansConfiguration!.ClusterOptions!.ClusterId;
                            options.ServiceId = this.ServiceConfiguration.OrleansConfiguration!.ClusterOptions!.ServiceId;
                        });

                    client.UseAdoNetClustering(
                        options =>
                        {
                            options.Invariant = GlobalConstants.Invariant;
                            options.ConnectionString = this.ServiceConfiguration.DatabaseSettings!.ToConnectionString();
                        });

                    // Wait for the silo, e.g. if the silo host and the cluster are started at the same time.
                    client.UseConnectionRetryFilter(
                        async (exception, cancellationToken) =>
                        {
                            if (++connectionAttempts >= MaximumConnectionAttempts)
                            {
                                return false;
                            }

                            this.Logger.Warning("Connecting to the silo failed (attempt {Attempt}), retrying: {Message}", connectionAttempts, exception.Message);
                            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                            return true;
                        });
                })
            .UseSerilog()
            .Build();
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
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            Path.Combine(currentLocation, "certificate.pfx"),
            "test");

        var optionsBuilder = this.mqttServerFactory
                .CreateServerOptionsBuilder();

        optionsBuilder = this.ServiceConfiguration.UnencryptedPort is null
                             ? optionsBuilder.WithoutDefaultEndpoint()
                             : optionsBuilder.WithDefaultEndpoint()
                                 .WithDefaultEndpointPort(this.ServiceConfiguration.UnencryptedPort.Value);

        optionsBuilder
                .WithEncryptedEndpoint()
                .WithEncryptedEndpointPort(this.ServiceConfiguration.Port)
                .WithEncryptionCertificate(certificate)
                .WithEncryptionSslProtocol(SslProtocols.Tls12);

        this.mqttServer = new MqttServerFactory().CreateMqttServer(optionsBuilder.Build());

        // Register the MQTT server events.
        this.mqttServer.ClientUnsubscribedTopicAsync += this.ClientUnsubscribed;
        this.mqttServer.InterceptingSubscriptionAsync += this.InterceptSubscription;
        this.mqttServer.InterceptingPublishAsync += this.InterceptPublish;
        this.mqttServer.ValidatingConnectionAsync += this.ValidateConnection;
        this.mqttServer.ClientDisconnectedAsync += this.ClientDisconnected;

        await this.mqttServer.StartAsync();
        await this.ConnectBroker();

        this.logger.Information("Started MQTT service.");
    }
}
