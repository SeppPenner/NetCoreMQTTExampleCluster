// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WhitelistRepository.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dapper;

    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Statements;

    using Npgsql;


    /// <inheritdoc cref="IWhitelistRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
    /// </summary>
    /// <seealso cref="IWhitelistRepository" />
    public class WhitelistRepository : IWhitelistRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WhitelistRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public WhitelistRepository(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IWhitelistRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="BlacklistWhitelist" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhitelistRepository" />
        public async Task<List<BlacklistWhitelist>> GetAllWhitelistItems()
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var whitelistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectAllWhitelistItems);
            return whitelistItems?.ToList() ?? new List<BlacklistWhitelist>();
        }

        /// <inheritdoc cref="IWhitelistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its identifier.
        /// </summary>
        /// <param name="whitelistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhitelistRepository" />
        public async Task<BlacklistWhitelist> GetWhitelistItemById(Guid whitelistItemId)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemById, new {Id = whitelistItemId});
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="whitelistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <param name="whitelistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<BlacklistWhitelist> GetWhitelistItemByIdAndType(Guid whitelistItemId, BlacklistWhitelistType whitelistItemType)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemByIdAndType, new {Id = whitelistItemId, Type = whitelistItemType});
        }

        /// <inheritdoc cref="IWhitelistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="whitelistItemType">The <see cref="BlacklistWhitelist" />'s type to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhitelistRepository" />
        public async Task<BlacklistWhitelist> GetWhitelistItemByType(BlacklistWhitelistType whitelistItemType)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemByType, new {Type = whitelistItemType});
        }

        /// <inheritdoc cref="IWhitelistRepository" />
        /// <summary>
        ///     Inserts a <see cref="BlacklistWhitelist" /> item to the database.
        /// </summary>
        /// <param name="whitelistItem">The <see cref="BlacklistWhitelist" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhitelistRepository" />
        public async Task<bool> InsertWhitelistItem(BlacklistWhitelist whitelistItem)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(InsertStatements.InsertWhitelistItem, whitelistItem);
            return result == 1;
        }
    }
}
