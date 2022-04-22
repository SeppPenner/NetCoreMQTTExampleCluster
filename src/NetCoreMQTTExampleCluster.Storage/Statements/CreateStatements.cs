// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateStatements.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for table creation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements
{
    /// <summary>
    /// The SQL statements for table creation.
    /// </summary>
    public class CreateStatements
    {
        /// <summary>
        /// A SQL query string to create the database.
        /// </summary>
        public static string CreateDatabase = @"CREATE DATABASE @Database;";

        /// <summary>
        /// A SQL query string to create the event log table.
        /// </summary>
        public static string CreateEventLogTable =
            @"CREATE TABLE IF NOT EXISTS eventlog (
                id                      UUID            NOT NULL,
                eventtype               INTEGER         NOT NULL,
                eventdetails            TEXT            NULL,
                createdat               TIMESTAMPTZ     DEFAULT now() NOT NULL
            );";

        /// <summary>
        /// A SQL query string to create the publish message table.
        /// </summary>
        public static string CreatePublishMessageTable =
            @"CREATE TABLE IF NOT EXISTS publishmessage (
                id                      UUID            NOT NULL,
                clientid                TEXT            NOT NULL,
                topic                   TEXT            NOT NULL,
                payload                 JSONB           NOT NULL,
                qos                     INTEGER         NOT NULL,
                retain                  BOOLEAN         NOT NULL,
                createdat               TIMESTAMPTZ     DEFAULT now() NOT NULL
            );";

        /// <summary>
        ///     A SQL query string to create the database version table.
        /// </summary>
        public static string CreateDatabaseVersionTable =
            @"CREATE TABLE IF NOT EXISTS databaseversion (
                id                      UUID            NOT NULL PRIMARY KEY,
                name                    TEXT            NOT NULL,
                number                  BIGINT          NOT NULL,
                createdat               TIMESTAMPTZ     DEFAULT now() NOT NULL
            );";

        /// <summary>
        ///     A SQL query string to create the blacklist table.
        /// </summary>
        public static string CreateBlacklistTable =
            @"CREATE TABLE IF NOT EXISTS blacklist (
                id                      UUID            NOT NULL PRIMARY KEY,
                userid                  UUID            NOT NULL                REFERENCES mqttuser(id),
                type                    INTEGER         NOT NULL,
                value                   TEXT            NOT NULL,
                createdat               TIMESTAMPTZ     DEFAULT now() NOT NULL,
                updatedat               TIMESTAMPTZ     NULL,
                deletedat               TIMESTAMPTZ     NULL
            );";

        /// <summary>
        ///     A SQL query string to create the whitelist table.
        /// </summary>
        public static string CreateWhitelistTable =
            @"CREATE TABLE IF NOT EXISTS whitelist (
                id                      UUID            NOT NULL PRIMARY KEY,
                userid                  UUID            NOT NULL                REFERENCES mqttuser(id),
                type                    INTEGER         NOT NULL,
                value                   TEXT            NOT NULL,
                createdat               TIMESTAMPTZ     DEFAULT now() NOT NULL,
                updatedat               TIMESTAMPTZ     NULL,
                deletedat               TIMESTAMPTZ     NULL
            );";

        /// <summary>
        ///     A SQL query string to create the user table.
        /// </summary>
        public static string CreateUserTable =
            @"CREATE TABLE IF NOT EXISTS mqttuser (
                id                                      UUID            NOT NULL PRIMARY KEY,
                username                                TEXT            NOT NULL UNIQUE,
                passwordhash                            TEXT            NOT NULL,
                clientidprefix                          TEXT            NULL,
                clientid                                TEXT            NULL,
                validateclientid                        BOOLEAN         DEFAULT false,
                throttleuser                            BOOLEAN         DEFAULT false,
                monthlybytelimit                        BIGINT          NULL,
                issyncuser                              BOOL            DEFAULT false,
                description                             TEXT            NULL,
                createdat                               TIMESTAMPTZ     DEFAULT now() NOT NULL,
                updatedat                               TIMESTAMPTZ     NULL,
                deletedat                               TIMESTAMPTZ     NULL
            );";

        /// <summary>
        ///     A SQL query string to enable the TimeScaleDB extension.
        /// </summary>
        public static string EnableTimeScaleDbExtension =
            @"CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;";

        /// <summary>
        ///     A SQL query string to create a hyper table from the event log table.
        /// </summary>
        public static string CreateEventLogHyperTable = @"SELECT create_hypertable('eventlog', 'createdat');";

        /// <summary>
        ///     A SQL query string to create a hyper table from the publish message table.
        /// </summary>
        public static string CreatePublishMessageHyperTable = @"SELECT create_hypertable('publishmessage', 'createdat');";

        /// <summary>
        ///     A SQL query string to create a compound index for timestamp and city identifier for the weather data table.
        /// </summary>
        public static string CreatePublishMessageCompoundIndex = @"CREATE INDEX ON publishmessage (createdat DESC, clientid);";
    }
}
