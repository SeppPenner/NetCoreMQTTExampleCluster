// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttPersistenceQueue.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface to queue the event logs and published messages of all grains in one silo for the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains;

/// <summary>
/// An interface to queue the event logs and published messages of all grains in one silo for the database.
/// </summary>
public interface IMqttPersistenceQueue
{
    /// <summary>
    /// Queues an event log entry.
    /// </summary>
    /// <param name="eventLog">The event log entry.</param>
    void Enqueue(EventLog eventLog);

    /// <summary>
    /// Queues a published message.
    /// </summary>
    /// <param name="publishMessage">The published message.</param>
    void Enqueue(PublishMessage publishMessage);
}
