// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerForwarder.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   Keeps one MQTT connection to another broker and publishes the forwarded messages over it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster;

/// <summary>
/// Keeps one MQTT connection to another broker and publishes the forwarded messages over it in the order they were queued.
/// </summary>
public sealed class BrokerForwarder : IAsyncDisposable
{
    /// <summary>
    /// The maximum number of messages that wait for the other broker. If the queue is full, the oldest message is dropped.
    /// </summary>
    private const int MaximumQueuedMessages = 10_000;

    /// <summary>
    /// The delay before a failed connection or publish is tried again.
    /// </summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The time the queued messages get to reach the other broker when the forwarder is stopped.
    /// </summary>
    private static readonly TimeSpan DrainTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The queued messages.
    /// </summary>
    private readonly Channel<MqttApplicationMessage> messages;

    /// <summary>
    /// The MQTT client.
    /// </summary>
    private readonly IMqttClient mqttClient;

    /// <summary>
    /// The MQTT client options.
    /// </summary>
    private readonly MqttClientOptions mqttClientOptions;

    /// <summary>
    /// The cancellation token source that stops the worker.
    /// </summary>
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// The worker that publishes the queued messages.
    /// </summary>
    private readonly Task worker;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// The number of messages dropped because the queue was full, since the last report in the log.
    /// </summary>
    private long droppedMessages;

    /// <summary>
    /// A value indicating whether the last connection or publish attempt failed.
    /// </summary>
    private bool failing;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerForwarder" /> class.
    /// </summary>
    /// <param name="brokerId">The identifier of the other broker.</param>
    /// <param name="brokerConnectionSettings">The settings to connect to the other broker.</param>
    /// <param name="clientId">The client identifier for the connection.</param>
    public BrokerForwarder(Guid brokerId, IBrokerConnectionSettings brokerConnectionSettings, string clientId)
    {
        this.logger = Log.ForContext<BrokerForwarder>()
            .ForContext("TargetBrokerId", brokerId)
            .ForContext("TargetBroker", $"{brokerConnectionSettings.HostName}:{brokerConnectionSettings.Port}");

        this.messages = Channel.CreateBounded<MqttApplicationMessage>(
            new BoundedChannelOptions(MaximumQueuedMessages)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true
            },
            _ => Interlocked.Increment(ref this.droppedMessages));

        // MQTT 5 keeps the message properties such as the content type and the user properties.
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithTcpServer(brokerConnectionSettings.HostName, brokerConnectionSettings.Port)
            .WithCredentials(brokerConnectionSettings.UserName, brokerConnectionSettings.Password)
            .WithCleanSession(brokerConnectionSettings.UseCleanSession)
            .WithProtocolVersion(MqttProtocolVersion.V500);

        if (brokerConnectionSettings.UseTls)
        {
            optionsBuilder.WithTlsOptions(options => options.UseTls());
        }

        this.mqttClientOptions = optionsBuilder.Build();
        this.mqttClient = new MqttClientFactory().CreateMqttClient();
        this.worker = Task.Run(() => this.PublishQueuedMessages(this.cancellationTokenSource.Token));
    }

    /// <summary>
    /// Queues a message for the other broker.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Enqueue(MqttApplicationMessage message)
    {
        this.messages.Writer.TryWrite(message);
    }

    /// <inheritdoc cref="IAsyncDisposable" />
    public async ValueTask DisposeAsync()
    {
        // Give the queued messages a short time to reach the other broker, then stop.
        this.messages.Writer.TryComplete();
        await Task.WhenAny(this.worker, Task.Delay(DrainTimeout));
        await this.cancellationTokenSource.CancelAsync();
        await this.worker;

        try
        {
            if (this.mqttClient.IsConnected)
            {
                await this.mqttClient.DisconnectAsync();
            }
        }
        catch (Exception ex)
        {
            this.logger.Debug("Disconnecting from the broker failed: {Message}", ex.Message);
        }

        this.mqttClient.Dispose();
        this.cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Publishes the queued messages until the forwarder is stopped.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    private async Task PublishQueuedMessages(CancellationToken cancellationToken)
    {
        try
        {
            while (await this.messages.Reader.WaitToReadAsync(cancellationToken))
            {
                while (this.messages.Reader.TryRead(out var message))
                {
                    // A message that couldn't be published is published again once the connection is back.
                    while (!await this.TryPublish(message, cancellationToken))
                    {
                        await Task.Delay(RetryDelay, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The forwarder is stopped.
        }
    }

    /// <summary>
    /// Connects to the other broker if needed and publishes one message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value indicating whether the message is done, <c>false</c> if it has to be published again.</returns>
    private async Task<bool> TryPublish(MqttApplicationMessage message, CancellationToken cancellationToken)
    {
        try
        {
            if (!this.mqttClient.IsConnected)
            {
                await this.mqttClient.ConnectAsync(this.mqttClientOptions, cancellationToken);
                this.logger.Information("Connected to the broker for forwarding.");
            }

            var result = await this.mqttClient.PublishAsync(message, cancellationToken);

            if (!result.IsSuccess)
            {
                // The other broker refused the message, publishing it again wouldn't change that.
                this.logger.Warning("The broker refused the message on topic {Topic}: {ReasonCode}.", message.Topic, result.ReasonCode);
            }

            if (this.failing)
            {
                this.failing = false;
                this.logger.Information("Forwarding to the broker works again.");
            }

            var dropped = Interlocked.Exchange(ref this.droppedMessages, 0);

            if (dropped > 0)
            {
                this.logger.Warning("Dropped {Count} messages for the broker because the queue was full.", dropped);
            }

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Log the first failure only, the forwarder retries every few seconds.
            if (!this.failing)
            {
                this.failing = true;
                this.logger.Warning("Forwarding to the broker failed, retrying every {RetryDelay}: {Message}", RetryDelay, ex.Message);
            }

            return false;
        }
    }
}
