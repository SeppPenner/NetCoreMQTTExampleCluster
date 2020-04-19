// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseVersionRepository.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="DatabaseVersion" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Dapper;

    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Statements;

    using Npgsql;

    /// <inheritdoc cref="IDatabaseVersionRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="DatabaseVersion" />s.
    /// </summary>
    /// <seealso cref="IDatabaseVersionRepository" />
    public class DatabaseVersionRepository : IDatabaseVersionRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseVersionRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public DatabaseVersionRepository(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IDatabaseVersionRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="DatabaseVersion" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseVersionRepository" />
        public async Task<IEnumerable<DatabaseVersion>> GetDatabaseVersions()
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryAsync<DatabaseVersion>(SelectStatements.SelectAllDatabaseVersions);
        }

        /// <inheritdoc cref="IDatabaseVersionRepository" />
        /// <summary>
        ///     Gets a <see cref="DatabaseVersion" /> by its identifier.
        /// </summary>
        /// <param name="databaseVersionId">The <see cref="DatabaseVersion" />'s identifier.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseVersionRepository" />
        public async Task<DatabaseVersion> GetDatabaseVersionById(Guid databaseVersionId)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<DatabaseVersion>(SelectStatements.SelectDatabaseVersionById, new {Id = databaseVersionId});
        }

        /// <inheritdoc cref="IDatabaseVersionRepository" />
        /// <summary>
        ///     Gets a <see cref="DatabaseVersion" /> by its name.
        /// </summary>
        /// <param name="databaseVersionName">The <see cref="DatabaseVersion" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseVersionRepository" />
        public async Task<DatabaseVersion> GetDatabaseVersionByName(string databaseVersionName)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<DatabaseVersion>(SelectStatements.SelectDatabaseVersionByName, new {DatabaseVersionName = databaseVersionName});
        }

        /// <inheritdoc cref="IDatabaseVersionRepository" />
        /// <summary>
        ///     Inserts a <see cref="DatabaseVersion" /> to the database.
        /// </summary>
        /// <param name="package">The <see cref="DatabaseVersion" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseVersionRepository" />
        public async Task<bool> InsertDatabaseVersion(DatabaseVersion package)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(InsertStatements.InsertDatabaseVersion, package);
            return result == 1;
        }
    }
}
