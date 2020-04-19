// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The user class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data
{
    using System;

    /// <summary>
    ///     The user class.
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets a salted and hashed representation of the password.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        ///     Gets or sets the client identifier prefix.
        /// </summary>
        public string ClientIdPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the client identifier is validated or not.
        /// </summary>
        public bool ValidateClientId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the user is throttled after a certain limit or not.
        /// </summary>
        public bool ThrottleUser { get; set; }

        /// <summary>
        ///     Gets or sets a user's monthly limit in byte.
        /// </summary>
        public long? MonthlyByteLimit { get; set; }

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
        ///     Returns a <see cref="string"></see> representation of the <see cref="User" /> class.
        /// </summary>
        /// <returns>A <see cref="string"></see> representation of the <see cref="User" /> class.</returns>
        public override string ToString()
        {
            return
                $"{{{nameof(this.Id)}: {this.Id}, {nameof(this.UserName)}: {this.UserName}, {nameof(this.ClientIdPrefix)}: {this.ClientIdPrefix}, {nameof(this.ClientId)}: {this.ClientId}, {nameof(this.ValidateClientId)}: {this.ValidateClientId}, {nameof(this.ClientId)}: {this.ClientId}, {nameof(this.ThrottleUser)}: {this.ThrottleUser}, {nameof(this.MonthlyByteLimit)}: {this.MonthlyByteLimit}, {nameof(this.CreatedAt)}: {this.CreatedAt}, {nameof(this.DeletedAt)}: {this.DeletedAt}, {nameof(this.UpdatedAt)}: {this.UpdatedAt}}}";
        }
    }
}
