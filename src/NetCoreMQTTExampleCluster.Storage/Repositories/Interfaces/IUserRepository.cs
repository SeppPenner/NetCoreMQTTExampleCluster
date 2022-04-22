// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Storage.Data;

    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="User" />s.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<User>> GetUsers();

        /// <summary>
        ///     Gets a <see cref="User" /> by their identifier.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<User> GetUserById(Guid userId);

        /// <summary>
        ///     Gets a <see cref="User" /> by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<User> GetUserByName(string userName);

        /// <summary>
        ///     Gets a <see cref="User" />'s name and identifier by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<(string, Guid)> GetUserNameAndUserIdByName(string userName);

        /// <summary>
        ///     Gets a <see cref="bool" /> value indicating whether the user name already exists or not.
        /// </summary>
        /// <param name="userName">The user name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> UserNameExists(string userName);

        /// <summary>
        ///     Inserts a <see cref="User" /> to the database.
        /// </summary>
        /// <param name="user">The <see cref="User" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertUser(User user);

        /// <summary>
        ///     Gets the blacklist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type);

        /// <summary>
        ///     Gets the whitelist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type);

        /// <summary>
        ///     Gets the client identifier prefixes for all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<string>> GetAllClientIdPrefixes();

        /// <summary>
        /// Gets a <see cref="User" />'s <see cref="UserData"/> by their identifier.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<UserData> GetUserData(Guid userId);
    }
}
