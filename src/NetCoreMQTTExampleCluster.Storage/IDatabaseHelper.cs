// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDatabaseHelper.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An interface to work with the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface to work with the database.
    /// </summary>
    public interface IDatabaseHelper
    {
        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="database">The database to create.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task CreateDatabase(string database);

        /// <summary>
        /// Deletes the database.
        /// </summary>
        /// <param name="database">The database to delete.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task DeleteDatabase(string database);

        /// <summary>
        /// Creates the event log table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateEventLogTable(bool forceDelete);

        /// <summary>
        /// Creates the publish message table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreatePublishMessageTable(bool forceDelete);

        /// <summary>
        /// Creates all tables.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool"/> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task CreateAllTables(bool forceDelete);

        /// <summary>
        ///     Creates the database version table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateDatabaseVersionTable(bool forceDelete);

        /// <summary>
        ///     Creates the whitelist table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateWhitelistTable(bool forceDelete);

        /// <summary>
        ///     Creates the blacklist table.
        /// </summary>
        /// <param name="forceDelete">A <see cref="bool" /> value to force the deletion of the table.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateBlacklistTable(bool forceDelete);

        /// <summary>
        /// Enables the TimeScaleDB extension.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task EnableTimeScaleDbExtension();

        /// <summary>
        /// Created the Orleans tables.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateOrleansTables();

        /// <summary>
        /// Enables the TimeScaleDB extension.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateHyperTables();

        /// <summary>
        /// Creates a compound index for the publish message table on timestamp and client identifier.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task CreateCompoundIndex();
    }
}
