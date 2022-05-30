// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectStatements.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for selecting data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements;

/// <summary>
/// The SQL statements for selecting data.
/// </summary>
public class SelectStatements
{
    /// <summary>
    /// A SQL query string to select all publish messages.
    /// </summary>
    public const string SelectAllPublishMessages =
        @"SELECT id, clientid, topic, payload, qos, retain, createdAt
            FROM publishmessage;";

    /// <summary>
    /// A SQL query string to select the publish message by its identifier.
    /// </summary>
    public const string SelectPublishMessageById =
        @"SELECT id, clientid, topic, payload, qos, retain, createdAt
            FROM publishmessage
            WHERE id = @Id;";

    /// <summary>
    /// A SQL query string to select all event logs.
    /// </summary>
    public const string SelectAllEventLogs =
        @"SELECT id, name, eventtype, eventdetails, createdat
            FROM eventlog;";

    /// <summary>
    /// A SQL query string to select the event log by its identifier.
    /// </summary>
    public const string SelectEventLogById =
        @"SELECT id, name, createdat, deletedat, updatedat
            FROM eventlog
            WHERE id = @Id
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select all users.
    /// </summary>
    public const string SelectAllUsers =
        @"SELECT id, username, passwordhash, clientidprefix, clientid, validateclientid, throttleuser, monthlybytelimit, issyncuser, description, createdat, updatedat, deletedat
            FROM mqttuser
			WHERE deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select all client id prefixes for all users.
    /// </summary>
    public const string SelectAllClientIdPrefixes =
        @"SELECT clientidprefix
            FROM mqttuser
            WHERE clientidprefix IS NOT NULL;";

    /// <summary>
    /// A SQL query string to select the MQTT user by their identifier.
    /// </summary>
    public const string SelectUserById =
        @"SELECT id, username, passwordhash, clientidprefix, clientid, validateclientid, throttleuser, monthlybytelimit, issyncuser, description, createdat, updatedat, deletedat
            FROM mqttuser
            WHERE id = @Id
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select the MQTT user by their user name.
    /// </summary>
    public const string SelectUserByUserName =
        @"SELECT id, username, passwordhash, clientidprefix, clientid, validateclientid, throttleuser, monthlybytelimit, issyncuser, description, createdat, updatedat, deletedat
            FROM mqttuser
            WHERE username = @UserName
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select the MQTT user's name and identifier by their user name.
    /// </summary>
    public const string SelectUserNameAndIdByUserName =
        @"SELECT username, id
            FROM mqttuser
            WHERE username = @UserName;";

    /// <summary>
    /// A SQL query string to select all database versions.
    /// </summary>
    public const string SelectAllDatabaseVersions =
        @"SELECT id, name, number, createdat
            FROM databaseversion;";

    /// <summary>
    /// A SQL query string to select the database version by its identifier.
    /// </summary>
    public const string SelectDatabaseVersionById =
        @"SELECT id, name, number, createdat
            FROM databaseversion
            WHERE id = @Id;";

    /// <summary>
    /// A SQL query string to select the database version by its name.
    /// </summary>
    public const string SelectDatabaseVersionByName =
        @"SELECT id, name, number, createdat
            FROM databaseversion
            WHERE name = @WidgetName;";

    /// <summary>
    /// A SQL query string to select all whitelist items.
    /// </summary>
    public const string SelectAllWhitelistItems =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM whitelist
			WHERE deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a whitelist item by its id.
    /// </summary>
    public const string SelectWhitelistItemById =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM whitelist
            WHERE id = @Id
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a whitelist item by its id and type.
    /// </summary>
    public const string SelectWhitelistItemByIdAndType =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM whitelist
            WHERE id = @Id
            AND type = @Type
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a whitelist item by its type.
    /// </summary>
    public const string SelectWhitelistItemByType =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM whitelist
            WHERE type = @Type
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select all blacklist items.
    /// </summary>
    public const string SelectAllBlacklistItems =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM blacklist
			WHERE deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a blacklist item by its id.
    /// </summary>
    public const string SelectBlacklistItemById =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM blacklist
            WHERE id = @Id
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a blacklist item by its id and type.
    /// </summary>
    public const string SelectBlacklistItemByIdAndType =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM blacklist
            WHERE id = @Id
            AND type = @Type
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select a blacklist item by its type.
    /// </summary>
    public const string SelectBlacklistItemByType =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM blacklist
            WHERE type = @Type
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select all blacklist items for a MQTT user.
    /// </summary>
    public const string SelectBlacklistItemsForUser =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM blacklist
            WHERE userid = @UserId AND type = @Type
			AND deletedat IS NULL;";

    /// <summary>
    /// A SQL query string to select all whitelist items for a MQTT user.
    /// </summary>
    public const string SelectWhitelistItemsForUser =
        @"SELECT id, userid, type, value, createdat, updatedat, deletedat
            FROM whitelist
            WHERE userid = @UserId AND type = @Type
			AND deletedat IS NULL;";
}
