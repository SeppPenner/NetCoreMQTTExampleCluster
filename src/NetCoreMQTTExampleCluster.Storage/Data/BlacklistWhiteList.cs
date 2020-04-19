// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlacklistWhitelist.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The blacklist or whitelist class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data
{
    using System;

    /// <summary>
    ///     The blacklist or whitelist class.
    /// </summary>
    public class BlacklistWhitelist
    {
        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the user identifier.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        ///     Gets or sets the blacklist or whitelist type.
        /// </summary>
        public BlacklistWhitelistType Type { get; set; }

        /// <summary>
        ///     Gets or sets the blacklist or whitelist value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the created at timestamp.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the deleted at timestamp.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        ///     Gets or sets the updated at timestamp.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; } = null;

        /// <summary>
        ///     Returns a <see cref="string"></see> representation of the <see cref="BlacklistWhitelist" /> class.
        /// </summary>
        /// <returns>A <see cref="string"></see> representation of the <see cref="BlacklistWhitelist" /> class.</returns>
        public override string ToString()
        {
            return $"{{{nameof(this.Id)}: {this.Id}, {nameof(this.UserId)}: {this.UserId}, {nameof(this.Type)}: {this.Type}, {nameof(this.Value)}: {this.Value}, {nameof(this.CreatedAt)}: {this.CreatedAt}, {nameof(this.DeletedAt)}: {this.DeletedAt}, {nameof(this.UpdatedAt)}: {this.UpdatedAt}}}";
        }
    }
}
