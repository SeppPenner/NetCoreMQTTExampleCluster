// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWhitelistRepository.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NetCoreMQTTExampleCluster.Storage.Data;

    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="BlacklistWhitelist" />s.
    /// </summary>
    public interface IWhitelistRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="BlacklistWhitelist" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<BlacklistWhitelist>> GetAllWhitelistItems();

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its identifier.
        /// </summary>
        /// <param name="whitelistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetWhitelistItemById(Guid whitelistItemId);

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="whitelistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetWhitelistItemByType(BlacklistWhitelistType whitelistItemType);

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="whitelistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <param name="whitelistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetWhitelistItemByIdAndType(Guid whitelistItemId, BlacklistWhitelistType whitelistItemType);

        /// <summary>
        ///     Inserts a <see cref="BlacklistWhitelist" /> item to the database.
        /// </summary>
        /// <param name="whitelistItem">The <see cref="BlacklistWhitelist" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertWhitelistItem(BlacklistWhitelist whitelistItem);
    }
}
