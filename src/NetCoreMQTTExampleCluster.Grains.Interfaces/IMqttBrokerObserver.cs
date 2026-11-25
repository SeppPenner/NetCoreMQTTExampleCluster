// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttBrokerObserver.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The observer interface of a broker that the repository grain tells about changes of the broker list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// The observer interface of a broker that the repository grain tells about changes of the broker list.
/// </summary>
public interface IMqttBrokerObserver : IGrainObserver
{
    /// <summary>
    /// Tells the broker that a broker connected or disconnected.
    /// </summary>
    /// <param name="brokers">All registered brokers by their broker identifier, the receiving broker included.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [OneWay]
    Task BrokersChanged(Dictionary<Guid, IBrokerConnectionSettings> brokers);
}
