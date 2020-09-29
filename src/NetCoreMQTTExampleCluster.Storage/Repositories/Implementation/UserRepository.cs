// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="User" />s.
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


    /// <inheritdoc cref="IUserRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="User" />s.
    /// </summary>
    /// <seealso cref="IUserRepository" />
    public class UserRepository : IUserRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly MqttDatabaseConnectionSettings connectionSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public UserRepository(MqttDatabaseConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<User>> GetUsers()
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var users = await connection.QueryAsync<User>(SelectStatements.SelectAllUsers);
            return users?.ToList() ?? new List<User>();
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="User" /> by their identifier.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<User> GetUserById(Guid userId)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<User>(SelectStatements.SelectUserById, new {Id = userId});
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="User" /> by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<User> GetUserByName(string userName)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<User>(SelectStatements.SelectUserByUserName, new {UserName = userName});
        }

        /// <summary>
        ///     Gets a <see cref="User" />'s name and identifier by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public async Task<(string, Guid)> GetUserNameAndUserIdByName(string userName)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<(string, Guid)>(SelectStatements.SelectUserNameAndIdByUserName, new {UserName = userName});
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="bool" /> value indicating whether the user name already exists or not.
        /// </summary>
        /// <param name="userName">The user name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<bool> UserNameExists(string userName)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<bool>(ExistsStatements.UserNameExists, new {UserName = userName});
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Inserts a <see cref="User" /> to the database.
        /// </summary>
        /// <param name="user">The <see cref="User" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<bool> InsertUser(User user)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(InsertStatements.InsertUser, user);
            return result == 1;
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the blacklist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var blacklistItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectBlacklistItemsForUser, new { UserId = userId, Type = type });
            return blacklistItems?.ToList() ?? new List<BlacklistWhitelist>();
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the whitelist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var whiteListItems = await connection.QueryAsync<BlacklistWhitelist>(SelectStatements.SelectWhitelistItemsForUser, new { UserId = userId, Type = type });
            return whiteListItems?.ToList() ?? new List<BlacklistWhitelist>();
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the client identifier prefixes for all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<string>> GetAllClientIdPrefixes()
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
            await connection.OpenAsync();
            var clientIdPrefixes = await connection.QueryAsync<string>(SelectStatements.SelectAllClientIdPrefixes);
            return clientIdPrefixes?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets a <see cref="User" />'s <see cref="UserData"/> by their identifier.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
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
}
