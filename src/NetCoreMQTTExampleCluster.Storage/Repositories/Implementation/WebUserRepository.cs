// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebUserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="WebUser" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IWebUserRepository" />
/// <summary>
/// An implementation supporting the repository pattern to work with <see cref="WebUser" />s.
/// </summary>
public class WebUserRepository : BaseRepository, IWebUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebUserRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public WebUserRepository(IMqttDatabaseConnectionSettings connectionSettings): base(connectionSettings)
    {
    }

    /// <inheritdoc cref="IWebUserRepository" />
    public async Task<List<WebUser>> GetWebUsers()
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var users = await connection.QueryAsync<WebUser>(SelectStatements.SelectAllWebUsers);
        return users?.ToList() ?? new List<WebUser>();
    }

    /// <inheritdoc cref="IWebUserRepository" />
    public async Task<WebUser> GetWebUserById(Guid userId)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<WebUser>(SelectStatements.SelectWebUserById, new {Id = userId});
    }

    /// <inheritdoc cref="IWebUserRepository" />
    public async Task<WebUser> GetWebUserByName(string userName)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<WebUser>(SelectStatements.SelectWebUserByUserName, new {UserName = userName});
    }

    /// <inheritdoc cref="IWebUserRepository" />
    public async Task<bool> InsertWebUser(WebUser webUser)
    {
        await using var connection = await this.GetDatabaseConnection().ConfigureAwait(false);
        var result = await connection.ExecuteAsync(InsertStatements.InsertWebUser, webUser);
        return result == 1;
    }
}
