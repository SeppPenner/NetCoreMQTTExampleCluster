// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertStatements.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for inserting data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The SQL statements for inserting data.
    /// </summary>
    public class InsertStatements
    {
        /// <summary>
        /// A SQL query string to insert a publish message.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertPublishMessage =
            @"INSERT INTO publishmessage (id, clientid, topic, payload, qos, retain)
            VALUES (@Id, @ClientId, @Topic, JSON(@Payload), @QoS, @Retain);";

        /// <summary>
        /// A SQL query string to insert an event log.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertEventLog =
            @"INSERT INTO eventlog (id, eventtype, eventdetails)
            VALUES (@Id, @EventType, @EventDetails);";

        /// <summary>
        ///     A SQL query string to insert a user.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertUser =
            @"INSERT INTO mqttuser (id, username, passwordhash, clientidprefix, clientid, validateclientid, throttleuser, monthlybytelimit, issyncuser)
            VALUES (@Id, @UserName, @PasswordHash, @ClientIdPrefix, @ClientId, @ValidateClientId, @ThrottleUser, @MonthlyByteLimit, @IsSyncUser);";

        /// <summary>
        ///     A SQL query string to insert a database version.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertDatabaseVersion =
            @"INSERT INTO databaseversion (id, name, number)
            VALUES (@Id, @Name, @Number);";

        /// <summary>
        ///     A SQL query string to insert a blacklist item.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertBlacklistItem =
            @"INSERT INTO blacklist (id, userid, type, value)
            VALUES (@Id, @UserId, @Type, @Value);";

        /// <summary>
        ///     A SQL query string to insert a whitelist item.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public static string InsertWhitelistItem =
            @"INSERT INTO whitelist (id, userid, type, value)
            VALUES (@Id, @UserId, @Type, @Value);";
    }
}
