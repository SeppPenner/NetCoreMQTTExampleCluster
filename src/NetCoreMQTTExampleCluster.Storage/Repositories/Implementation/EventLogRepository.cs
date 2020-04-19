// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventLogRepository.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="Log" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation
{
    using System;
    using System.Collections.Generic;

    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Statements;
    using System.Threading.Tasks;

    using Dapper;

    using Npgsql;

    /// <inheritdoc cref="IEventLogRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="EventLog" />s.
    /// </summary>
    /// <seealso cref="IEventLogRepository" />
    public class EventLogRepository : IEventLogRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventLogRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public EventLogRepository(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IEventLogRepository" />
        /// <summary>
        /// Gets a <see cref="List{T}"/> of all <see cref="EventLog"/>s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IEventLogRepository" />
        public async Task<IEnumerable<EventLog>> GetEventLogs()
        {
            using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryAsync<EventLog>(SelectStatements.SelectAllEventLogs);
        }

        /// <inheritdoc cref="IEventLogRepository" />
        /// <summary>
        ///     Gets a <see cref="EventLog" /> by its identifier.
        /// </summary>
        /// <param name="eventLogId">The <see cref="EventLog"/>'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IEventLogRepository" />
        public async Task<EventLog> GetEventLogById(Guid eventLogId)
        {
            using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<EventLog>(SelectStatements.SelectEventLogById, new { Id = eventLogId });
        }

        /// <inheritdoc cref="IEventLogRepository" />
        /// <summary>
        ///     Inserts a <see cref="EventLog" /> to the database.
        /// </summary>
        /// <param name="eventLog">The <see cref="EventLog" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IEventLogRepository" />
        public async Task<bool> InsertEventLog(EventLog eventLog)
        {
            using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(InsertStatements.InsertEventLog, eventLog);
            return result == 1;
        }
    }
}
