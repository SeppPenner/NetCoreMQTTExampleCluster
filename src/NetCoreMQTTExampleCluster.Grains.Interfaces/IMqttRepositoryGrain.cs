// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttRepositoryGrain.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The grain interface for a repository to manage the brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// The grain interface for a repository to manage the brokers.
/// </summary>
public interface IMqttRepositoryGrain : IGrainWithIntegerKey
{
    /// <summary>
    /// Connects a broker to the grain. The brokers call this cyclically, because the grain keeps the brokers in memory only.
    /// </summary>
    /// <param name="brokerConnectionSettings">The settings the other brokers use to connect to this broker.</param>
    /// <param name="brokerId">The broker identifier.</param>
    /// <param name="observer">The observer the grain tells about changes of the broker list.</param>
    /// <returns>All registered brokers by their broker identifier, the calling broker included.</returns>
    Task<Dictionary<Guid, IBrokerConnectionSettings>> ConnectBroker(IBrokerConnectionSettings brokerConnectionSettings, Guid brokerId, IMqttBrokerObserver observer);

    /// <summary>
    /// Disconnects the broker from the grain.
    /// </summary>
    /// <param name="brokerId">The broker identifier.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    Task DisconnectBroker(Guid brokerId);
}
