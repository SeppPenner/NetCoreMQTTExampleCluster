// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseHelper.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation to work with the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage;

/// <inheritdoc cref="IDatabaseHelper" />
public class DatabaseHelper : BaseRepository, IDatabaseHelper
{
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext<DatabaseHelper>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHelper" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public DatabaseHelper(IMqttDatabaseConnectionSettings connectionSettings) : base(connectionSettings)
    {
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateDatabase(string database)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var checkDatabaseExists = await connection.ExecuteScalarAsync(ExistsStatements.CheckDatabaseExists, new { this.ConnectionSettings.Database });

        if (Convert.ToBoolean(checkDatabaseExists) == false)
        {
            this.logger.Information("The database doesn't exist. I'm creating it.");
            var sql = CreateStatements.CreateDatabase.Replace("@Database", database);
            await connection.ExecuteAsync(sql);
            this.logger.Information("Created database.");
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task DeleteDatabase(string database)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var sql = DropStatements.DropDatabase.Replace("@Database", database);
        await connection.ExecuteAsync(sql);
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateAllTables(bool forceDelete)
    {
        await this.CreateEventLogTable(forceDelete);
        await this.CreatePublishMessageTable(forceDelete);
        await this.CreateDatabaseVersionTable(forceDelete);
        await this.CreateWebUserTable(forceDelete);
        await this.CreateMqttUserTable(forceDelete);
        await this.CreateWhitelistTable(forceDelete);
        await this.CreateBlacklistTable(forceDelete);
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateEventLogTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the event log table.");
            await connection.ExecuteAsync(DropStatements.DropEventLogTable);
            this.logger.Information("Deleted event log table.");
            await connection.ExecuteAsync(CreateStatements.CreateEventLogTable);
            this.logger.Information("Created event log table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckEventLogTableExists);
            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The event log table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateEventLogTable);
                this.logger.Information("Created event log table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreatePublishMessageTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the publish message table.");
            await connection.ExecuteAsync(DropStatements.DropPublishMessageTable);
            this.logger.Information("Deleted publish message table.");
            await connection.ExecuteAsync(CreateStatements.CreatePublishMessageTable);
            this.logger.Information("Created publish message table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckPublishMessageTableExists);
            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The publish message table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreatePublishMessageTable);
                this.logger.Information("Created publish message table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateDatabaseVersionTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the database version table.");
            await connection.ExecuteAsync(DropStatements.DropDatabaseVersionTable);
            this.logger.Information("Deleted database version table.");
            await connection.ExecuteAsync(CreateStatements.CreateDatabaseVersionTable);
            this.logger.Information("Created database version table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckDatabaseVersionTableExists);

            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The database version table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateDatabaseVersionTable);
                this.logger.Information("Created database version table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateWhitelistTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the whitelist table.");
            await connection.ExecuteAsync(DropStatements.DropWhitelistTable);
            this.logger.Information("Deleted whitelist table.");
            await connection.ExecuteAsync(CreateStatements.CreateWhitelistTable);
            this.logger.Information("Created whitelist table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckWhitelistTableExists);

            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The whitelist table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateWhitelistTable);
                this.logger.Information("Created whitelist table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateBlacklistTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the blacklist table.");
            await connection.ExecuteAsync(DropStatements.DropBlacklistTable);
            this.logger.Information("Deleted blacklist table.");
            await connection.ExecuteAsync(CreateStatements.CreateBlacklistTable);
            this.logger.Information("Created blacklist table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckBlacklistTableExists);

            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The blacklist table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateBlacklistTable);
                this.logger.Information("Created blacklist table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateMqttUserTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the MQTT user table.");
            await connection.ExecuteAsync(DropStatements.DropMqttUserTable);
            this.logger.Information("Deleted MQTT user table.");
            await connection.ExecuteAsync(CreateStatements.CreateMqttUserTable);
            this.logger.Information("Created MQTT user table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckMqttUserTableExists);

            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The MQTT user table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateMqttUserTable);
                this.logger.Information("Created MQTT user table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateWebUserTable(bool forceDelete)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        if (forceDelete)
        {
            this.logger.Information("Force delete the web user table.");
            await connection.ExecuteAsync(DropStatements.DropWebUserTable);
            this.logger.Information("Deleted web user table.");
            await connection.ExecuteAsync(CreateStatements.CreateWebUserTable);
            this.logger.Information("Created web user table.");
        }
        else
        {
            var checkTableExistsResult = await connection.ExecuteScalarAsync(ExistsStatements.CheckWebUserTableExists);

            if (Convert.ToBoolean(checkTableExistsResult) == false)
            {
                this.logger.Information("The web user table doesn't exist. I'm creating it.");
                await connection.ExecuteAsync(CreateStatements.CreateWebUserTable);
                this.logger.Information("Created web user table.");
            }
        }
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task EnableTimeScaleDbExtension()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        this.logger.Information("Enabling TimeScaleDB extension.");
        await connection.ExecuteAsync(CreateStatements.EnableTimeScaleDbExtension);
        this.logger.Information("Enabled TimeScaleDB extension.");
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateHyperTables()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        this.logger.Information("Creating hyper tables.");
        await connection.ExecuteAsync(CreateStatements.CreateEventLogHyperTable);
        await connection.ExecuteAsync(CreateStatements.CreatePublishMessageHyperTable);
        this.logger.Information("Created hyper tables.");
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateOrleansTables()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        this.logger.Information("Creating Orleans tables.");

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

        this.logger.Information("Created Orleans tables.");
    }

    /// <inheritdoc cref="IDatabaseHelper" />
    public async Task CreateCompoundIndex()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        Log.Information("Creating compound index.");
        await connection.ExecuteAsync(CreateStatements.CreatePublishMessageCompoundIndex);
        Log.Information("Created compound index.");
    }

    /// <summary>
    /// Reads a SQL file from the resources.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>A <see cref="string"/> containing SQL statements from the resource.</returns>
    private static string ReadSqlFile(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream);
        var file = reader.ReadToEnd();

        return file;
    }
}
