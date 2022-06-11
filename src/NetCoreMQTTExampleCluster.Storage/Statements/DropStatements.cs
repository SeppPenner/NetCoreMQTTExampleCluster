// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropStatements.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The SQL statements for table deletion.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Statements;

/// <summary>
/// The SQL statements for table deletion.
/// </summary>
public class DropStatements
{
    /// <summary>
    /// A SQL query string to drop the database.
    /// </summary>
    public const string DropDatabase = @"DROP DATABASE IF EXISTS @Database;";

    /// <summary>
    /// A SQL query string to delete the publish message table.
    /// </summary>
    public const string DropPublishMessageTable = @"DROP TABLE IF EXISTS publishmessage;";

    /// <summary>
    /// A SQL query string to delete the event log table.
    /// </summary>
    public const string DropEventLogTable = @"DROP TABLE IF EXISTS eventlog;";

    /// <summary>
    /// A SQL query string to delete the database version table.
    /// </summary>
    public const string DropDatabaseVersionTable = @"DROP TABLE IF EXISTS databaseversion;";

    /// <summary>
    /// A SQL query string to delete the blacklist table.
    /// </summary>
    public const string DropBlacklistTable = @"DROP TABLE IF EXISTS blacklist;";

    /// <summary>
    /// A SQL query string to delete the whitelist table.
    /// </summary>
    public const string DropWhitelistTable = @"DROP TABLE IF EXISTS whitelist;";

    /// <summary>
    /// A SQL query string to delete the MQTT user table.
    /// </summary>
    public const string DropMqttUserTable = @"DROP TABLE IF EXISTS mqttuser;";

    /// <summary>
    /// A SQL query string to delete the web user table.
    /// </summary>
    public const string DropWebUserTable = @"DROP TABLE IF EXISTS webuser;";    
}
