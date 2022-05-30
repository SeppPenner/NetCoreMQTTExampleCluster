// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A program to setup the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.DatabaseSetup;

/// <summary>
/// A program to setup the database.
/// </summary>
public static class Program
{
    /// <summary>
    /// The database version repository.
    /// </summary>
    [NotNull]
    private static IDatabaseVersionRepository? databaseVersionRepository = null;

    /// <summary>
    /// The whitelist repository.
    /// </summary>
    [NotNull]
    private static IWhitelistRepository? whitelistRepository = null;

    /// <summary>
    /// The MQTT user repository.
    /// </summary>
    [NotNull]
    private static IMqttUserRepository? mqttUserRepository = null;

    /// <summary>
    /// The database helper.
    /// </summary>
    [NotNull]
    private static IDatabaseHelper? databaseHelper = null;

    /// <summary>
    /// The main method of the program.
    /// </summary>
    /// <returns>A <see cref="Task" />.</returns>
    public static async Task Main()
    {
        var connectionSettings = await ReadSettingsFile();

        mqttUserRepository = new MqttUserRepository(connectionSettings);
        databaseVersionRepository = new DatabaseVersionRepository(connectionSettings);
        whitelistRepository = new WhitelistRepository(connectionSettings);
        databaseHelper = new DatabaseHelper(connectionSettings);

        Console.WriteLine("Setting up the database...");

        Console.WriteLine("Deleting database...");
        await databaseHelper.DeleteDatabase(connectionSettings.Database);

        Console.WriteLine("Creating database...");
        await databaseHelper.CreateDatabase(connectionSettings.Database);

        Console.WriteLine("Adding TimeScaleDB extension...");
        await databaseHelper.EnableTimeScaleDbExtension();

        Console.WriteLine("Adding Orleans tables...");
        await databaseHelper.CreateOrleansTables();

        Console.WriteLine("Creating tables...");
        await databaseHelper.CreateAllTables(true);

        Console.WriteLine("Creating hyper tables...");
        await databaseHelper.CreateHyperTables();

        Console.WriteLine("Creating compound index...");
        await databaseHelper.CreateCompoundIndex();

        Console.WriteLine("Adding seed data...");
        await SeedData();

        Console.WriteLine("Press any key to close this window...");
        Console.ReadKey();
    }

    /// <summary>
    /// Reads the settings file.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task<MqttDatabaseConnectionSettings> ReadSettingsFile()
    {
        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var settingsFile = Path.Combine(currentLocation, "appsettings.json");
        var settingsString = await File.ReadAllTextAsync(settingsFile);
        return JsonConvert.DeserializeObject<MqttDatabaseConnectionSettings>(settingsString);
    }

    /// <summary>
    /// Seeds some data to the database.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task SeedData()
    {
        await InsertDatabaseVersion();
        var (user1Id, user2Id) = await InsertUsers();
        await AddWhiteAndBlackListsForFirstUser(user1Id);
        await AddWhiteAndBlackListsForSecondUser(user2Id);
    }

    /// <summary>
    /// Adds the blacklist and whitelist items for the first MQTT user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task AddWhiteAndBlackListsForFirstUser(Guid userId)
    {
        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Publish,
            UserId = userId,
            Value = "a/b"
        });

        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Publish,
            UserId = userId,
            Value = "a/d"
        });

        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Subscribe,
            UserId = userId,
            Value = "d/e"
        });

        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Subscribe,
            UserId = userId,
            Value = "d"
        });
    }

    /// <summary>
    /// Adds the blacklist and whitelist items for the second user: mqtt-broker-sync.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task AddWhiteAndBlackListsForSecondUser(Guid userId)
    {
        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Publish,
            UserId = userId,
            Value = "#"
        });

        await whitelistRepository.InsertWhitelistItem(new BlacklistWhitelist
        {
            Type = BlacklistWhitelistType.Subscribe,
            UserId = userId,
            Value = "#"
        });
    }

    /// <summary>
    /// Inserts the <see cref="MqttUser"/>s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task<(Guid, Guid)> InsertUsers()
    {
        var passwordHasher = new PasswordHasher<MqttUser>();

        // Add test MQTT user
        var userId = Guid.NewGuid();

        var mqttUser = new MqttUser
        {
            Id = userId,
            UserName = "Test",
            ValidateClientId = false
        };

        mqttUser.PasswordHash = passwordHasher.HashPassword(mqttUser, "test");
        await mqttUserRepository.InsertMqttUser(mqttUser);

        // Add broker MQTT user
        var user2Id = Guid.NewGuid();

        var mqttUser2 = new MqttUser
        {
            Id = user2Id,
            UserName = "mqtt-broker-sync",
            ClientIdPrefix = "mqtt-broker-sync",
            ValidateClientId = true,
            IsSyncUser = true
        };

        mqttUser2.PasswordHash = passwordHasher.HashPassword(mqttUser2, "Test");
        await mqttUserRepository.InsertMqttUser(mqttUser2);
        return (userId, user2Id);
    }

    /// <summary>
    /// Inserts the <see cref="DatabaseVersion"/>.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task InsertDatabaseVersion()
    {
        var databaseVersions = await databaseVersionRepository.GetDatabaseVersions() ?? new List<DatabaseVersion>();

        if (!databaseVersions.Any())
        {
            await databaseVersionRepository.InsertDatabaseVersion(
                new DatabaseVersion
                {
                    Name = "1.0.0",
                    Number = 100
                });
        }
    }
}
