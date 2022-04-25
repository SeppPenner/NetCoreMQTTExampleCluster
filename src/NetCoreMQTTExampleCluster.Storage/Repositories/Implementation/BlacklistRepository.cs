// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlacklistRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IBlacklistRepository" />
public class BlacklistRepository : BaseRepository, IBlacklistRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public BlacklistRepository(IMqttDatabaseConnectionSettings connectionSettings) : base(connectionSettings)
    {
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<List<BlacklistWhitelist>> GetAllBlacklistItems()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var blacklistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectAllBlacklistItems);
        return blacklistItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemById(Guid blacklistItemId)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemById, new {Id = blacklistItemId});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemByIdAndType(Guid blacklistItemId, BlacklistWhitelistType blacklistItemType)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemByIdAndType, new {Id = blacklistItemId, Type = blacklistItemType});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemByType(BlacklistWhitelistType blacklistItemType)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemByType, new {Type = blacklistItemType});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    public async Task<bool> InsertBlacklistItem(BlacklistWhitelist blacklistItem)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var result = await connection.ExecuteAsync(InsertStatements.InsertBlacklistItem, blacklistItem);
        return result == 1;
    }
}
