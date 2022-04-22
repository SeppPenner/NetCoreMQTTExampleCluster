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
/// <summary>
/// An implementation supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
/// </summary>
/// <seealso cref="IBlacklistRepository" />
public class BlacklistRepository : IBlacklistRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public BlacklistRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    /// Gets a <see cref="List{T}" /> of all <see cref="BlacklistWhitelist" /> items.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IBlacklistRepository" />
    public async Task<List<BlacklistWhitelist>> GetAllBlacklistItems()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var blacklistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectAllBlacklistItems);
        return blacklistItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    /// Gets a <see cref="BlacklistWhitelist" /> item by its identifier.
    /// </summary>
    /// <param name="blacklistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemById(Guid blacklistItemId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemById, new {Id = blacklistItemId});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    /// Gets a <see cref="BlacklistWhitelist" /> item by its type.
    /// </summary>
    /// <param name="blacklistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
    /// <param name="blacklistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemByIdAndType(Guid blacklistItemId, BlacklistWhitelistType blacklistItemType)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemByIdAndType, new {Id = blacklistItemId, Type = blacklistItemType});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    /// Gets a <see cref="BlacklistWhitelist" /> item by its type.
    /// </summary>
    /// <param name="blacklistItemType">The <see cref="BlacklistWhitelist" />'s type to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IBlacklistRepository" />
    public async Task<BlacklistWhitelist> GetBlacklistItemByType(BlacklistWhitelistType blacklistItemType)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemByType, new {Type = blacklistItemType});
    }

    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    /// Inserts a <see cref="BlacklistWhitelist" /> item to the database.
    /// </summary>
    /// <param name="blacklistItem">The <see cref="BlacklistWhitelist" /> item to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IBlacklistRepository" />
    public async Task<bool> InsertBlacklistItem(BlacklistWhitelist blacklistItem)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertBlacklistItem, blacklistItem);
        return result == 1;
    }
}
