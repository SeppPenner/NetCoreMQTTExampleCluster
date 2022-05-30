// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

/// <summary>
/// An interface supporting the repository pattern to work with <see cref="MqttUser" />s.
/// </summary>
public interface IMqttUserRepository
{
    /// <summary>
    /// Gets a <see cref="List{T}" /> of all <see cref="MqttUser" />s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<MqttUser>> GetUsers();

    /// <summary>
    /// Gets a <see cref="MqttUser" /> by their identifier.
    /// </summary>
    /// <param name="userId">The <see cref="MqttUser" />'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<MqttUser> GetUserById(Guid userId);

    /// <summary>
    /// Gets a <see cref="MqttUser" /> by their user name.
    /// </summary>
    /// <param name="userName">The <see cref="MqttUser" />'s name to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<MqttUser> GetUserByName(string userName);

    /// <summary>
    /// Gets a <see cref="MqttUser" />'s name and identifier by their user name.
    /// </summary>
    /// <param name="userName">The <see cref="MqttUser" />'s name to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<(string, Guid)> GetUserNameAndUserIdByName(string userName);

    /// <summary>
    /// Gets a <see cref="bool" /> value indicating whether the user name already exists or not.
    /// </summary>
    /// <param name="userName">The user name to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> UserNameExists(string userName);

    /// <summary>
    /// Inserts a <see cref="MqttUser" /> to the database.
    /// </summary>
    /// <param name="mqttUser">The <see cref="MqttUser" /> to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertUser(MqttUser mqttUser);

    /// <summary>
    /// Gets the blacklist items for a <see cref="MqttUser" />.
    /// </summary>
    /// <param name="userId">The user identifier to query for.</param>
    /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type);

    /// <summary>
    /// Gets the whitelist items for a <see cref="MqttUser" />.
    /// </summary>
    /// <param name="userId">The user identifier to query for.</param>
    /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type);

    /// <summary>
    /// Gets the client identifier prefixes for all <see cref="MqttUser" />s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<string>> GetAllClientIdPrefixes();

    /// <summary>
    /// Gets a <see cref="MqttUser" />'s <see cref="MqttUserData"/> by their identifier.
    /// </summary>
    /// <param name="userId">The <see cref="MqttUser" />'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<MqttUserData> GetUserData(Guid userId);
}
