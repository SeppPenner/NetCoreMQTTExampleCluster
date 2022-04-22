// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IUserRepository" />
/// <summary>
/// An implementation supporting the repository pattern to work with <see cref="User" />s.
/// </summary>
public class UserRepository : IUserRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public UserRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<List<User>> GetUsers()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var users = await connection.QueryAsync<User>(SelectStatements.SelectAllUsers);
        return users?.ToList() ?? new List<User>();
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<User> GetUserById(Guid userId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(SelectStatements.SelectUserById, new {Id = userId});
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<User> GetUserByName(string userName)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(SelectStatements.SelectUserByUserName, new {UserName = userName});
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<(string, Guid)> GetUserNameAndUserIdByName(string userName)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<(string, Guid)>(SelectStatements.SelectUserNameAndIdByUserName, new {UserName = userName});
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<bool> UserNameExists(string userName)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<bool>(ExistsStatements.UserNameExists, new {UserName = userName});
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<bool> InsertUser(User user)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertUser, user);
        return result == 1;
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var blacklistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemsForUser, new { UserId = userId, Type = type });
        return blacklistItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var whiteListItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemsForUser, new { UserId = userId, Type = type });
        return whiteListItems?.ToList() ?? new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<List<string>> GetAllClientIdPrefixes()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var clientIdPrefixes = await connection.QueryAsync<string>(SelectStatements.SelectAllClientIdPrefixes);
        return clientIdPrefixes?.ToList() ?? new List<string>();
    }

    /// <inheritdoc cref="IUserRepository" />
    public async Task<UserData> GetUserData(Guid userId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();

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

        return new UserData
        {
            ClientIdPrefixes = clientIdPrefixes.ToList(),
            SubscriptionWhitelist = subscriptionWhitelist.ToList(),
            SubscriptionBlacklist = subscriptionBlacklist.ToList(),
            PublishWhitelist = publishWhitelist.ToList(),
            PublishBlacklist = publishBlacklist.ToList()
        };
    }
}
