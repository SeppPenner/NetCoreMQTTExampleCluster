// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseVersionRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="DatabaseVersion" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IDatabaseVersionRepository" />
public class DatabaseVersionRepository : IDatabaseVersionRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseVersionRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public DatabaseVersionRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
    }

    /// <inheritdoc cref="IDatabaseVersionRepository" />
    public async Task<List<DatabaseVersion>> GetDatabaseVersions()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var databaseVersions = await connection.QueryAsync<DatabaseVersion>(SelectStatements.SelectAllDatabaseVersions);
        return databaseVersions?.ToList() ?? new List<DatabaseVersion>();
    }

    /// <inheritdoc cref="IDatabaseVersionRepository" />
    public async Task<DatabaseVersion> GetDatabaseVersionById(Guid databaseVersionId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<DatabaseVersion>(SelectStatements.SelectDatabaseVersionById, new {Id = databaseVersionId});
    }

    /// <inheritdoc cref="IDatabaseVersionRepository" />
    public async Task<DatabaseVersion> GetDatabaseVersionByName(string databaseVersionName)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<DatabaseVersion>(SelectStatements.SelectDatabaseVersionByName, new {DatabaseVersionName = databaseVersionName});
    }

    /// <inheritdoc cref="IDatabaseVersionRepository" />
    public async Task<bool> InsertDatabaseVersion(DatabaseVersion package)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertDatabaseVersion, package);
        return result == 1;
    }
}
