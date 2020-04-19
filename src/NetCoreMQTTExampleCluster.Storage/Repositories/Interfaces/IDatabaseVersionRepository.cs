// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDatabaseVersionRepository.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="DatabaseVersion" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Storage.Data;

    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="DatabaseVersion" />s.
    /// </summary>
    public interface IDatabaseVersionRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="DatabaseVersion" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<IEnumerable<DatabaseVersion>> GetDatabaseVersions();

        /// <summary>
        ///     Gets a <see cref="DatabaseVersion" /> by its identifier.
        /// </summary>
        /// <param name="databaseVersionId">The The <see cref="DatabaseVersion" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<DatabaseVersion> GetDatabaseVersionById(Guid databaseVersionId);

        /// <summary>
        ///     Gets a <see cref="DatabaseVersion" /> by its name.
        /// </summary>
        /// <param name="databaseVersionName">The <see cref="DatabaseVersion" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<DatabaseVersion> GetDatabaseVersionByName(string databaseVersionName);

        /// <summary>
        ///     Inserts a <see cref="DatabaseVersion" /> to the database.
        /// </summary>
        /// <param name="databaseVersion">The <see cref="DatabaseVersion" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertDatabaseVersion(DatabaseVersion databaseVersion);
    }
}
