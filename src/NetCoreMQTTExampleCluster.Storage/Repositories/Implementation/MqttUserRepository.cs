// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttUserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IMqttUserRepository" />
/// <summary>
/// An implementation supporting the repository pattern to work with <see cref="MqttUser" />s.
/// </summary>
public class MqttUserRepository: BaseRepository, IMqttUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttUserRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public MqttUserRepository(IMqttDatabaseConnectionSettings connectionSettings): base(connectionSettings)
    {
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<MqttUser>> GetUsers()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var users = await connection.QueryAsync<MqttUser>(SelectStatements.SelectAllUsers);
        return users?.ToList() ?? new List<MqttUser>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUser> GetUserById(Guid userId)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<MqttUser>(SelectStatements.SelectUserById, new {Id = userId});
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUser> GetUserByName(string userName)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<MqttUser>(SelectStatements.SelectUserByUserName, new {UserName = userName});
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<(string, Guid)> GetUserNameAndUserIdByName(string userName)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<(string, Guid)>(SelectStatements.SelectUserNameAndIdByUserName, new {UserName = userName});
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<bool> UserNameExists(string userName)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<bool>(ExistsStatements.UserNameExists, new {UserName = userName});
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<bool> InsertUser(MqttUser mqttUser)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var result = await connection.ExecuteAsync(InsertStatements.InsertUser, mqttUser);
        return result == 1;
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var blacklistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemsForUser, new { UserId = userId, Type = type });
        return blacklistItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var whiteListItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemsForUser, new { UserId = userId, Type = type });
        return whiteListItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<string>> GetAllClientIdPrefixes()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var clientIdPrefixes = await connection.QueryAsync<string>(SelectStatements.SelectAllClientIdPrefixes);
        return clientIdPrefixes?.ToList() ?? new List<string>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUserData> GetUserData(Guid userId)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);

        var clientIdPrefixes = await connection.QueryAsync<string>(SelectStatements.SelectAllClientIdPrefixes);
        var subscriptionWhitelist = await connection.QueryAsync<BlacklistWhitelist>(
                                        SelectStatements.SelectWhitelistItemsForUser,
                                        new { UserId = userId, Type = BlacklistWhitelistType.Subscribe });
        var subscriptionBlacklist = await connection.QueryAsync<BlacklistWhitelist>(
                                        SelectStatements.SelectBlacklistItemsForUser,
                                        new { UserId = userId, Type = BlacklistWhitelistType.Subscribe });
        var publishWhitelist = await connection.QueryAsync<BlacklistWhitelist>(
                                   SelectStatements.SelectWhitelistItemsForUser,
                                   new { UserId = userId, Type = BlacklistWhitelistType.Publish });
        var publishBlacklist = await connection.QueryAsync<BlacklistWhitelist>(
                                   SelectStatements.SelectBlacklistItemsForUser,
                                   new { UserId = userId, Type = BlacklistWhitelistType.Publish });

        return new MqttUserData
        {
            ClientIdPrefixes = clientIdPrefixes.ToList(),
            SubscriptionWhitelist = subscriptionWhitelist.ToList(),
            SubscriptionBlacklist = subscriptionBlacklist.ToList(),
            PublishWhitelist = publishWhitelist.ToList(),
            PublishBlacklist = publishBlacklist.ToList()
        };
    }
}
