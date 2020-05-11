// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseVersion.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The database version class. It contains information about the database version used.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data
{
    using System;

    /// <summary>
    ///     The database version class. It contains information about the database version used.
    /// </summary>
    public class DatabaseVersion
    {
        /// <summary>
        ///     Gets or sets the primary key.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the version name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the version number.
        /// </summary>
        public long Number { get; set; }

        /// <summary>
        ///     Gets or sets the created at timestamp.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        ///     Returns a <see cref="string"></see> representation of the <see cref="DatabaseVersion" /> class.
        /// </summary>
        /// <returns>A <see cref="string"></see> representation of the <see cref="DatabaseVersion" /> class.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
