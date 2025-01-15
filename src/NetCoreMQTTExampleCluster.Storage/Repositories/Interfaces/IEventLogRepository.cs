// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventLogRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="EventLog" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

/// <summary>
/// An interface supporting the repository pattern to work with <see cref="EventLog"/>s.
/// </summary>
public interface IEventLogRepository
{
    /// <summary>
    /// Gets a <see cref="List{T}"/> of all <see cref="EventLog"/>s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<EventLog>> GetEventLogs();

    /// <summary>
    /// Gets a <see cref="EventLog" /> by its identifier.
    /// </summary>
    /// <param name="eventLogId">The <see cref="EventLog"/>'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<EventLog?> GetEventLogById(Guid eventLogId);

    /// <summary>
    /// Inserts a <see cref="EventLog" /> to the database.
    /// </summary>
    /// <param name="eventLog">The <see cref="EventLog" /> to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertEventLog(EventLog eventLog);

    /// <summary>
    /// Inserts a <see cref="List{T}"/> of <see cref="EventLog" />s to the database.
    /// </summary>
    /// <param name="eventLogs">The <see cref="List{T}"/> of<see cref="EventLog" />s to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertEventLogs(List<EventLog> eventLogs);
}
