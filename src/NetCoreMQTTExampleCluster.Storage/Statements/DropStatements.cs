// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropStatements.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for table deletion.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The SQL statements for table deletion.
    /// </summary>
    public class DropStatements
    {
        /// <summary>
        /// A SQL query string to drop the database.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropDatabase = @"DROP DATABASE IF EXISTS @Database;";

        /// <summary>
        /// A SQL query string to delete the publish message table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropPublishMessageTable = @"DROP TABLE IF EXISTS publishmessage;";

        /// <summary>
        /// A SQL query string to delete the event log table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropEventLogTable = @"DROP TABLE IF EXISTS eventlog;";

        /// <summary>
        ///     A SQL query string to delete the database version table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropDatabaseVersionTable = @"DROP TABLE IF EXISTS databaseversion;";

        /// <summary>
        ///     A SQL query string to delete the blacklist table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropBlacklistTable = @"DROP TABLE IF EXISTS blacklist;";

        /// <summary>
        ///     A SQL query string to delete the whitelist table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropWhitelistTable = @"DROP TABLE IF EXISTS whitelist;";

        /// <summary>
        ///     A SQL query string to delete the user table.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string DropUserTable = @"DROP TABLE IF EXISTS mqttuser;";
    }
}
