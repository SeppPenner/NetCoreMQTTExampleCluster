// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventLogRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="Log" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IEventLogRepository" />
public class EventLogRepository : BaseRepository, IEventLogRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public EventLogRepository(MqttDatabaseConnectionSettings connectionSettings) : base(connectionSettings)
    {
    }

    /// <inheritdoc cref="IEventLogRepository" />
    public async Task<List<EventLog>> GetEventLogs()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var eventLogs = await connection.QueryAsync<EventLog>(SelectStatements.SelectAllEventLogs);
        return eventLogs?.ToList() ?? new List<EventLog>();
    }

    /// <inheritdoc cref="IEventLogRepository" />
    public async Task<EventLog> GetEventLogById(Guid eventLogId)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<EventLog>(SelectStatements.SelectEventLogById, new { Id = eventLogId });
    }

    /// <inheritdoc cref="IEventLogRepository" />
    public async Task<bool> InsertEventLog(EventLog eventLog)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var result = await connection.ExecuteAsync(InsertStatements.InsertEventLog, eventLog);
        return result == 1;
    }

    /// <inheritdoc cref="IEventLogRepository" />
    public async Task<bool> InsertEventLogs(List<EventLog> eventLogs)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var result = await connection.ExecuteAsync(InsertStatements.InsertEventLog, eventLogs);
        return result == 1;
    }
}
