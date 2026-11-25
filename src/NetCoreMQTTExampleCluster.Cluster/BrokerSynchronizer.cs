// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerSynchronizer.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   Forwards the accepted messages of this broker to all other brokers of the cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster;

/// <inheritdoc cref="IMqttBrokerObserver" />
/// <summary>
/// Forwards the accepted messages of this broker to all other brokers of the cluster, one <see cref="BrokerForwarder"/> per broker.
/// </summary>
public sealed class BrokerSynchronizer : IMqttBrokerObserver, IAsyncDisposable
{
    /// <summary>
    /// The identifier of this broker.
    /// </summary>
    private readonly Guid brokerId;

    /// <summary>
    /// The client identifier this broker uses for the connections to the other brokers.
    /// </summary>
    private readonly string clientId;

    /// <summary>
    /// The forwarders by the identifier of the other broker.
    /// </summary>
    private readonly Dictionary<Guid, BrokerForwarder> forwarders = [];

    /// <summary>
    /// The lock for the forwarders.
    /// </summary>
    private readonly Lock forwardersLock = new();

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext<BrokerSynchronizer>();

    /// <summary>
    /// A value indicating whether the synchronizer is disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrokerSynchronizer" /> class.
    /// </summary>
    /// <param name="brokerId">The identifier of this broker.</param>
    /// <param name="clientId">The client identifier this broker uses for the connections to the other brokers.</param>
    public BrokerSynchronizer(Guid brokerId, string clientId)
    {
        this.brokerId = brokerId;
        this.clientId = clientId;
    }

    /// <inheritdoc cref="IMqttBrokerObserver" />
    public Task BrokersChanged(Dictionary<Guid, IBrokerConnectionSettings> brokers)
    {
        this.UpdateBrokers(brokers);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts a forwarder for each new broker and stops the forwarders of brokers that are gone.
    /// </summary>
    /// <param name="brokers">All registered brokers by their broker identifier.</param>
    public void UpdateBrokers(IReadOnlyDictionary<Guid, IBrokerConnectionSettings> brokers)
    {
        var removedForwarders = new List<BrokerForwarder>();

        lock (this.forwardersLock)
        {
            if (this.disposed)
            {
                return;
            }

            // The settings of a broker identifier never change, a restarted broker gets a new identifier.
            foreach (var (otherBrokerId, settings) in brokers.Where(b => b.Key != this.brokerId && !this.forwarders.ContainsKey(b.Key)))
            {
                this.forwarders[otherBrokerId] = new BrokerForwarder(otherBrokerId, settings, this.clientId);
                this.logger.Information("Forwarding messages to broker {BrokerId} at {HostName}:{Port}.", otherBrokerId, settings.HostName, settings.Port);
            }

            foreach (var otherBrokerId in this.forwarders.Keys.Where(id => !brokers.ContainsKey(id)).ToList())
            {
                removedForwarders.Add(this.forwarders[otherBrokerId]);
                this.forwarders.Remove(otherBrokerId);
                this.logger.Information("Stopped forwarding messages to broker {BrokerId}.", otherBrokerId);
            }
        }

        foreach (var forwarder in removedForwarders)
        {
            forwarder.DisposeAsync().AsTask().Ignore();
        }
    }

    /// <summary>
    /// Forwards a message to all other brokers.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Forward(MqttApplicationMessage message)
    {
        var copy = CopyMessage(message);

        lock (this.forwardersLock)
        {
            foreach (var forwarder in this.forwarders.Values)
            {
                forwarder.Enqueue(copy);
            }
        }
    }

    /// <inheritdoc cref="IAsyncDisposable" />
    public async ValueTask DisposeAsync()
    {
        List<BrokerForwarder> stoppedForwarders;

        lock (this.forwardersLock)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            stoppedForwarders = [.. this.forwarders.Values];
            this.forwarders.Clear();
        }

        await Task.WhenAll(stoppedForwarders.Select(f => f.DisposeAsync().AsTask()));
    }

    /// <summary>
    /// Copies a message for the forwarding. The payload keeps its bytes unchanged, whatever they encode.
    /// The topic alias and the subscription identifiers belong to the original connection and are left out.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The copy of the message.</returns>
    private static MqttApplicationMessage CopyMessage(MqttApplicationMessage message)
    {
        return new MqttApplicationMessage
        {
            Topic = message.Topic,
            Payload = new ReadOnlySequence<byte>(message.Payload.ToArray()),
            QualityOfServiceLevel = message.QualityOfServiceLevel,
            Retain = message.Retain,
            ContentType = message.ContentType,
            CorrelationData = message.CorrelationData?.ToArray(),
            MessageExpiryInterval = message.MessageExpiryInterval,
            PayloadFormatIndicator = message.PayloadFormatIndicator,
            ResponseTopic = message.ResponseTopic,
            UserProperties = message.UserProperties?.Select(p => new MqttUserProperty(p.Name, p.ValueBuffer.ToArray())).ToList()
        };
    }
}
