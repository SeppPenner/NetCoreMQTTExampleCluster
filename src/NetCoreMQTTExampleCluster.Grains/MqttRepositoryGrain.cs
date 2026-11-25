// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttRepositoryGrain.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The grain for a repository to manage the brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains;

/// <inheritdoc cref="IMqttRepositoryGrain" />
public class MqttRepositoryGrain : Grain, IMqttRepositoryGrain
{
    /// <summary>
    /// The persistence queue.
    /// </summary>
    private readonly IMqttPersistenceQueue persistenceQueue;

    /// <summary>
    /// The brokers by their broker identifier.
    /// </summary>
    private readonly Dictionary<Guid, IBrokerConnectionSettings> brokers = [];

    /// <summary>
    /// The observers of the brokers by their broker identifier.
    /// </summary>
    private readonly Dictionary<Guid, IMqttBrokerObserver> observers = [];

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext("Grain", nameof(MqttRepositoryGrain));

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttRepositoryGrain" /> class.
    /// </summary>
    /// <param name="persistenceQueue">The persistence queue.</param>
    public MqttRepositoryGrain(IMqttPersistenceQueue persistenceQueue)
    {
        this.persistenceQueue = persistenceQueue;
    }

    /// <inheritdoc cref="IMqttRepositoryGrain" />
    public Task<Dictionary<Guid, IBrokerConnectionSettings>> ConnectBroker(IBrokerConnectionSettings brokerConnectionSettings, Guid brokerId, IMqttBrokerObserver observer)
    {
        ArgumentNullException.ThrowIfNull(brokerConnectionSettings);
        ArgumentNullException.ThrowIfNull(observer);

        if (brokerId == default)
        {
            throw new ArgumentNullException(nameof(brokerId));
        }

        // A broker that restarts after a crash gets a new identifier but keeps its endpoint. Its old entry is removed,
        // otherwise the other brokers would keep a second connection to that endpoint and forward every message twice.
        var replacedBrokerIds = this.brokers
            .Where(b => b.Key != brokerId && IsSameEndpoint(b.Value, brokerConnectionSettings))
            .Select(b => b.Key)
            .ToList();

        foreach (var replacedBrokerId in replacedBrokerIds)
        {
            this.brokers.Remove(replacedBrokerId);
            this.observers.Remove(replacedBrokerId);

            this.persistenceQueue.Enqueue(new EventLog
            {
                EventType = EventType.BrokerDisconnect,
                EventDetails = $"Broker replaced: BrokerId = {replacedBrokerId}, new BrokerId = {brokerId}."
            });
        }

        var isNewBroker = !this.brokers.ContainsKey(brokerId);
        this.brokers[brokerId] = brokerConnectionSettings;
        this.observers[brokerId] = observer;

        // The brokers register cyclically, so only new brokers are saved to the database and announced to the others.
        if (isNewBroker)
        {
            this.persistenceQueue.Enqueue(new EventLog
            {
                EventType = EventType.BrokerConnect,
                EventDetails = $"New broker connected: BrokerId = {brokerId}."
            });

            this.NotifyOtherBrokers(brokerId);
        }

        return Task.FromResult(this.GetBrokers());
    }

    /// <inheritdoc cref="IMqttRepositoryGrain" />
    public Task DisconnectBroker(Guid brokerId)
    {
        if (brokerId == default)
        {
            throw new ArgumentNullException(nameof(brokerId));
        }

        this.persistenceQueue.Enqueue(new EventLog
        {
            EventType = EventType.BrokerDisconnect,
            EventDetails = $"Broker disconnected: BrokerId = {brokerId}."
        });

        this.observers.Remove(brokerId);

        if (this.brokers.Remove(brokerId))
        {
            this.NotifyOtherBrokers(brokerId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks whether two brokers have the same endpoint for the forwarding.
    /// </summary>
    /// <param name="first">The settings of the first broker.</param>
    /// <param name="second">The settings of the second broker.</param>
    /// <returns>A value indicating whether the host name and the port are the same.</returns>
    private static bool IsSameEndpoint(IBrokerConnectionSettings first, IBrokerConnectionSettings second)
    {
        return string.Equals(first.HostName, second.HostName, StringComparison.OrdinalIgnoreCase) && first.Port == second.Port;
    }

    /// <summary>
    /// Gets a copy of the broker list.
    /// </summary>
    /// <returns>All registered brokers by their broker identifier.</returns>
    private Dictionary<Guid, IBrokerConnectionSettings> GetBrokers()
    {
        return new Dictionary<Guid, IBrokerConnectionSettings>(this.brokers);
    }

    /// <summary>
    /// Tells all brokers except the changed one that the broker list has changed.
    /// A broker that misses the notification gets the list with its next cyclic registration.
    /// </summary>
    /// <param name="changedBrokerId">The identifier of the broker that connected or disconnected.</param>
    private void NotifyOtherBrokers(Guid changedBrokerId)
    {
        var brokerList = this.GetBrokers();

        foreach (var (brokerId, observer) in this.observers.Where(o => o.Key != changedBrokerId))
        {
            try
            {
                observer.BrokersChanged(brokerList).Ignore();
            }
            catch (Exception ex)
            {
                this.logger.Warning("Notifying broker {BrokerId} about the broker list failed: {Message}", brokerId, ex.Message);
            }
        }
    }
}
