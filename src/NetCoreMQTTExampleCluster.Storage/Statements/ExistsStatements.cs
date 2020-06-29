// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExistsStatements.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for checking if a table exists.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The SQL statements for checking if a table exists.
    /// </summary>
    public class ExistsStatements
    {
        /// <summary>
        /// A SQL query string to check whether the database exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckDatabaseExists =
            @"SELECT EXISTS (
                SELECT datname
                FROM pg_catalog.pg_database
                WHERE datname = '@Database'
            );";

        /// <summary>
        /// A SQL query string to check whether the publish message table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckPublishMessageTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = 'public'
                AND    table_name = 'publishmessage'
            );";

        /// <summary>
        /// A SQL query string to check whether the event log table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckEventLogTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = '@Schema'
                AND    table_name = 'eventlog'
            );";

        /// <summary>
        ///     A SQL query string to check whether the database version table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckDatabaseVersionTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = 'public'
                AND    table_name = 'databaseversion'
            );";

        /// <summary>
        ///     A SQL query string to check whether the blacklist table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckBlacklistTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = 'public'
                AND    table_name = 'blacklist'
            );";

        /// <summary>
        ///     A SQL query string to check whether the whitelist table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckWhitelistTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = 'public'
                AND    table_name = 'whitelist'
            );";

        /// <summary>
        ///     A SQL query string to check whether the user table exists.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string CheckUserTableExists =
            @"SELECT EXISTS (
                SELECT 1
                FROM   information_schema.tables 
                WHERE  table_schema = 'public'
                AND    table_name = 'mqttuser'
            );";

        /// <summary>
        ///     A SQL query string to select whether a user name already exists or not.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string UserNameExists =
            @"SELECT EXISTS (
                SELECT username FROM user
                WHERE username = @UserName
            );";
    }
}
