// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWebUserRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="WebUser" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

/// <summary>
/// An interface supporting the repository pattern to work with <see cref="WebUser" />s.
/// </summary>
public interface IWebUserRepository
{
    /// <summary>
    /// Gets a <see cref="List{T}" /> of all <see cref="WebUser" />s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<WebUser>> GetWebUsers();

    /// <summary>
    /// Gets a <see cref="WebUser" /> by their identifier.
    /// </summary>
    /// <param name="userId">The <see cref="WebUser" />'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<WebUser> GetWebUserById(Guid userId);

    /// <summary>
    /// Gets a <see cref="WebUser" /> by their user name.
    /// </summary>
    /// <param name="userName">The <see cref="WebUser" />'s name to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<WebUser> GetWebUserByName(string userName);

    /// <summary>
    /// Inserts a <see cref="WebUser" /> to the database.
    /// </summary>
    /// <param name="webUser">The <see cref="WebUser" /> to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertWebUser(WebUser webUser);
}
