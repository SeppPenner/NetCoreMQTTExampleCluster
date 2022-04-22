// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlacklistRepository.cs" company="Hämmer Electronics">
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
    public interface IBlacklistRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="BlacklistWhitelist" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<List<BlacklistWhitelist>> GetAllBlacklistItems();

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its identifier.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetBlacklistItemById(Guid blacklistItemId);

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetBlacklistItemByType(BlacklistWhitelistType blacklistItemType);

        /// <summary>
        ///     Gets a <see cref="BlacklistWhitelist" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlacklistWhitelist" />'s identifier to query for.</param>
        /// <param name="blacklistItemType">The <see cref="BlacklistWhitelistType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlacklistWhitelist> GetBlacklistItemByIdAndType(Guid blacklistItemId, BlacklistWhitelistType blacklistItemType);

        /// <summary>
        ///     Inserts a <see cref="BlacklistWhitelist" /> item to the database.
        /// </summary>
        /// <param name="blacklistItem">The <see cref="BlacklistWhitelist" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertBlacklistItem(BlacklistWhitelist blacklistItem);
    }
}
