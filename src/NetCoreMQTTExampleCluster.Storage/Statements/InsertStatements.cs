﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertStatements.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for inserting data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements
{
    /// <summary>
    /// The SQL statements for inserting data.
    /// </summary>
    public class InsertStatements
    {
        /// <summary>
        /// A SQL query string to insert a publish message.
        /// </summary>
        public static string InsertPublishMessage =
            @"INSERT INTO publishmessage (id, clientid, topic, payload, qos, retain)
            VALUES (@Id, @ClientId, @Topic, JSON(@Payload), @QoS, @Retain);";

        /// <summary>
        /// A SQL query string to insert an event log.
        /// </summary>
        public static string InsertEventLog =
            @"INSERT INTO eventlog (id, eventtype, eventdetails)
            VALUES (@Id, @EventType, @EventDetails);";

        /// <summary>
        ///     A SQL query string to insert a user.
        /// </summary>
        public static string InsertUser =
            @"INSERT INTO mqttuser (id, username, passwordhash, clientidprefix, clientid, validateclientid, throttleuser, monthlybytelimit, issyncuser, description)
            VALUES (@Id, @UserName, @PasswordHash, @ClientIdPrefix, @ClientId, @ValidateClientId, @ThrottleUser, @MonthlyByteLimit, @IsSyncUser, @Description);";

        /// <summary>
        ///     A SQL query string to insert a database version.
        /// </summary>
        public static string InsertDatabaseVersion =
            @"INSERT INTO databaseversion (id, name, number)
            VALUES (@Id, @Name, @Number);";

        /// <summary>
        ///     A SQL query string to insert a blacklist item.
        /// </summary>
        public static string InsertBlacklistItem =
            @"INSERT INTO blacklist (id, userid, type, value)
            VALUES (@Id, @UserId, @Type, @Value);";

        /// <summary>
        ///     A SQL query string to insert a whitelist item.
        /// </summary>
        public static string InsertWhitelistItem =
            @"INSERT INTO whitelist (id, userid, type, value)
            VALUES (@Id, @UserId, @Type, @Value);";
    }
}
