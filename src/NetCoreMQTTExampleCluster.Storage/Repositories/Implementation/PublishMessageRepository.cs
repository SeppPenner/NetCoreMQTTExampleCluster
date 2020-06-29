// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishMessageRepository.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="PublishMessage" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Statements;
    using System.Threading.Tasks;

    using Dapper;

    using NetCoreMQTTExampleCluster.Storage.Mappers;

    using Npgsql;

    /// <inheritdoc cref="IPublishMessageRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="PublishMessage" />s.
    /// </summary>
    /// <seealso cref="IPublishMessageRepository" />
    public class PublishMessageRepository : IPublishMessageRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PublishMessageRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public PublishMessageRepository(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IPublishMessageRepository" />
        /// <summary>
        /// Gets a <see cref="List{T}"/> of all <see cref="PublishMessage"/>s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IPublishMessageRepository" />
        public async Task<List<PublishMessage>> GetPublishMessages()
        {
            SqlMapper.AddTypeHandler(typeof(PublishedMessagePayload), new JsonMapper<PublishedMessagePayload>());
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var publishMessages = await connection.QueryAsync<PublishMessage>(SelectStatements.SelectAllPublishMessages);
            return publishMessages?.ToList() ?? new List<PublishMessage>();
        }

        /// <inheritdoc cref="IPublishMessageRepository" />
        /// <summary>
        ///     Gets a <see cref="PublishMessage" /> by its identifier.
        /// </summary>
        /// <param name="publishMessageId">The <see cref="PublishMessage"/>'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IPublishMessageRepository" />
        public async Task<PublishMessage> GetPublishMessageById(Guid publishMessageId)
        {
            SqlMapper.AddTypeHandler(typeof(PublishedMessagePayload), new JsonMapper<PublishedMessagePayload>());
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<PublishMessage>(SelectStatements.SelectPublishMessageById, new { Id = publishMessageId });
        }

        /// <inheritdoc cref="IPublishMessageRepository" />
        /// <summary>
        ///     Inserts a <see cref="PublishMessage" /> to the database.
        /// </summary>
        /// <param name="publishMessage">The <see cref="PublishMessage" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IPublishMessageRepository" />
        public async Task<bool> InsertPublishMessage(PublishMessage publishMessage)
        {
            SqlMapper.AddTypeHandler(typeof(PublishedMessagePayload), new JsonMapper<PublishedMessagePayload>());
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(InsertStatements.InsertPublishMessage, publishMessage);
            return result == 1;
        }
    }
}
