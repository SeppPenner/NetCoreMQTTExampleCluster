// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseHelper.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation to work with the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dapper;

    using NetCoreMQTTExampleCluster.Storage.Statements;

    using Microsoft.AspNetCore.Authentication;

    using Newtonsoft.Json;

    using Npgsql;

    using Serilog;

    /// <summary>
    /// An implementation to work with the database.
    /// </summary>
    public class DatabaseHelper : IDatabaseHelper
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseHelper" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public DatabaseHelper(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="database">The database to create.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateDatabase(string database)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToAdminConnectionString());
            await connection.OpenAsync();
            var checkDatabaseExists = connection.ExecuteScalar(ExistsStatements.CheckDatabaseExists, new { this.connectionSettings.Database });

            if (Convert.ToBoolean(checkDatabaseExists) == false)
            {
                Log.Information("The database doesn't exist. I'm creating it.");
                var sql = CreateStatements.CreateDatabase.Replace("@Database", database);
                await connection.ExecuteAsync(sql);
                Log.Information("Created database.");
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Deletes the database.
        /// </summary>
        /// <param name="database">The database to delete.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task DeleteDatabase(string database)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToAdminConnectionString());
            await connection.OpenAsync();
            var sql = DropStatements.DropDatabase.Replace("@Database", database);
            await connection.ExecuteAsync(sql);
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Creates all tables.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateAllTables(bool forceDelete)
        {
            await this.CreateEventLogTable(forceDelete);
            await this.CreatePublishMessageTable(forceDelete);
            await this.CreateDatabaseVersionTable(forceDelete);
            await this.CreateUserTable(forceDelete);
            await this.CreateWhitelistTable(forceDelete);
            await this.CreateBlacklistTable(forceDelete);
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Creates the event log table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateEventLogTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the event log table.");
                await connection.ExecuteAsync(DropStatements.DropEventLogTable);
                Log.Information("Deleted event log table.");
                await connection.ExecuteAsync(CreateStatements.CreateEventLogTable);
                Log.Information("Created event log table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckEventLogTableExists);
                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The event log table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreateEventLogTable);
                    Log.Information("Created event log table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Creates the publish message table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreatePublishMessageTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the publish message table.");
                await connection.ExecuteAsync(DropStatements.DropPublishMessageTable);
                Log.Information("Deleted publish message table.");
                await connection.ExecuteAsync(CreateStatements.CreatePublishMessageTable);
                Log.Information("Created publish message table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckPublishMessageTableExists);
                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The publish message table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreatePublishMessageTable);
                    Log.Information("Created publish message table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        ///     Creates the database version table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateDatabaseVersionTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the database version table.");
                await connection.ExecuteAsync(DropStatements.DropDatabaseVersionTable);
                Log.Information("Deleted database version table.");
                await connection.ExecuteAsync(CreateStatements.CreateDatabaseVersionTable);
                Log.Information("Created database version table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckDatabaseVersionTableExists);

                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The database version table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreateDatabaseVersionTable);
                    Log.Information("Created database version table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        ///     Creates the whitelist table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateWhitelistTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the whitelist table.");
                await connection.ExecuteAsync(DropStatements.DropWhitelistTable);
                Log.Information("Deleted whitelist table.");
                await connection.ExecuteAsync(CreateStatements.CreateWhitelistTable);
                Log.Information("Created whitelist table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckWhitelistTableExists);

                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The whitelist table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreateWhitelistTable);
                    Log.Information("Created whitelist table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        ///     Creates the blacklist table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateBlacklistTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the blacklist table.");
                await connection.ExecuteAsync(DropStatements.DropBlacklistTable);
                Log.Information("Deleted blacklist table.");
                await connection.ExecuteAsync(CreateStatements.CreateBlacklistTable);
                Log.Information("Created blacklist table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckBlacklistTableExists);

                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The blacklist table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreateBlacklistTable);
                    Log.Information("Created blacklist table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        ///     Creates the user table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateUserTable(bool forceDelete)
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            if (forceDelete)
            {
                Log.Information("Force delete the user table.");
                await connection.ExecuteAsync(DropStatements.DropUserTable);
                Log.Information("Deleted user table.");
                await connection.ExecuteAsync(CreateStatements.CreateUserTable);
                Log.Information("Created user table.");
            }
            else
            {
                var checkTableExistsResult = connection.ExecuteScalar(ExistsStatements.CheckUserTableExists);

                if (Convert.ToBoolean(checkTableExistsResult) == false)
                {
                    Log.Information("The user table doesn't exist. I'm creating it.");
                    await connection.ExecuteAsync(CreateStatements.CreateUserTable);
                    Log.Information("Created user table.");
                }
            }
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Enables the TimeScaleDB extension.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task EnableTimeScaleDbExtension()
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            Log.Information("Enabling TimeScaleDB extension.");
            await connection.ExecuteAsync(CreateStatements.EnableTimeScaleDbExtension);
            Log.Information("Enabled TimeScaleDB extension.");
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Enables the TimeScaleDB extension.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateHyperTables()
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            Log.Information("Creating hyper tables.");
            await connection.ExecuteAsync(CreateStatements.CreateEventLogHyperTable);
            await connection.ExecuteAsync(CreateStatements.CreatePublishMessageHyperTable);
            Log.Information("Created hyper tables.");
        }

        /// <inheritdoc cref="IDatabaseHelper" />
        /// <summary>
        /// Created the Orleans tables.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IDatabaseHelper" />
        public async Task CreateOrleansTables()
        {
            await using var connection = new NpgsqlConnection(connectionSettings.ToConnectionString());
            await connection.OpenAsync();

            Log.Information("Creating Orleans tables.");

            // Load PostgreSQL-Main.sql
            var mainStatements = ReadSqlFile("NetCoreMQTTExampleCluster.Storage.OrleansQueries.PostgreSQL-Main.sql");

            if (!string.IsNullOrWhiteSpace(mainStatements))
            {
                await connection.ExecuteAsync(mainStatements);
            }

            // Load PostgreSQL-Clustering.sql
            var clusteringStatements = ReadSqlFile("NetCoreMQTTExampleCluster.Storage.OrleansQueries.PostgreSQL-Clustering.sql");

            if (!string.IsNullOrWhiteSpace(clusteringStatements))
            {
                await connection.ExecuteAsync(clusteringStatements);
            }

            // Load PostgreSQL-Reminders.sql
            var remindersStatements = ReadSqlFile("NetCoreMQTTExampleCluster.Storage.OrleansQueries.PostgreSQL-Reminders.sql");

            if (!string.IsNullOrWhiteSpace(remindersStatements))
            {
                await connection.ExecuteAsync(remindersStatements);
            }

            Log.Information("Created Orleans tables.");
        }

        /// <summary>
        /// Reads a SQL file from the resources.
        /// </summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>A <see cref="string"/> containing SQL statements from the resource.</returns>
        private static string ReadSqlFile(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string file;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return string.Empty;
                }

                using var reader = new StreamReader(stream);
                file = reader.ReadToEnd();
            }

            return file;
        }
    }
}
