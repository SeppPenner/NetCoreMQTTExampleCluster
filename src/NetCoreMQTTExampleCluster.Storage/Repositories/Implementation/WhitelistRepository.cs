// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WhitelistRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IWhitelistRepository" />
public class WhitelistRepository : IWhitelistRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhitelistRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public WhitelistRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
    }

    /// <inheritdoc cref="IWhitelistRepository" />
    public async Task<List<BlacklistWhitelist>> GetAllWhitelistItems()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var whitelistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectAllWhitelistItems);
        return whitelistItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IWhitelistRepository" />
    public async Task<BlacklistWhitelist> GetWhitelistItemById(Guid whitelistItemId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemById, new {Id = whitelistItemId});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetWhitelistItemByIdAndType(Guid whitelistItemId, BlacklistWhitelistType whitelistItemType)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemByIdAndType, new {Id = whitelistItemId, Type = whitelistItemType});
    }

    /// <inheritdoc cref="IWhitelistRepository" />
    public async Task<BlacklistWhitelist> GetWhitelistItemByType(BlacklistWhitelistType whitelistItemType)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemByType, new {Type = whitelistItemType});
    }

    /// <inheritdoc cref="IWhitelistRepository" />
    public async Task<bool> InsertWhitelistItem(BlacklistWhitelist whitelistItem)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertWhitelistItem, whitelistItem);
        return result == 1;
    }
}
