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
    /// The command line argument that deletes an existing database before it is created again.
    /// </summary>
    private const string DropDatabaseArgument = "--drop-database";

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
    /// <param name="args">The command line arguments. <c>--drop-database</c> deletes an existing database first.</param>
    /// <returns>A <see cref="Task" />.</returns>
    public static async Task Main(string[] args)
    {
        var dropDatabase = args.Contains(DropDatabaseArgument, StringComparer.OrdinalIgnoreCase);
        var connectionSettings = await ReadSettingsFile();

        if (connectionSettings is null)
        {
            throw new InvalidOperationException("The connection settings are null");
        }

        mqttUserRepository = new MqttUserRepository(connectionSettings);
        databaseVersionRepository = new DatabaseVersionRepository(connectionSettings);
        whitelistRepository = new WhitelistRepository(connectionSettings);
        databaseHelper = new DatabaseHelper(connectionSettings);

        Console.WriteLine("Setting up the database...");

        if (dropDatabase)
        {
            Console.WriteLine("Deleting database...");
            await databaseHelper.DeleteDatabase(connectionSettings.Database);
        }
        else
        {
            Console.WriteLine($"Keeping an existing database, pass {DropDatabaseArgument} to delete it first...");
        }

        Console.WriteLine("Creating database...");
        await databaseHelper.CreateDatabase(connectionSettings.Database);

        Console.WriteLine("Adding TimeScaleDB extension...");
        await databaseHelper.EnableTimeScaleDbExtension();

        Console.WriteLine("Adding Orleans tables...");
        await databaseHelper.CreateOrleansTables();

        Console.WriteLine("Creating tables...");
        await databaseHelper.CreateAllTables(false);

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
    private static async Task<MqttDatabaseConnectionSettings?> ReadSettingsFile()
    {
        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (string.IsNullOrWhiteSpace(currentLocation))
        {
            throw new InvalidOperationException("The current location is null or empty");
        }

        var settingsFile = Path.Combine(currentLocation, "appsettings.json");
        var settingsString = await File.ReadAllTextAsync(settingsFile);
        return JsonConvert.DeserializeObject<MqttDatabaseConnectionSettings?>(settingsString);
    }

    /// <summary>
    /// Seeds some data to the database.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    private static async Task SeedData()
    {
        await InsertDatabaseVersion();
        var (user1Id, user2Id) = await InsertUsers();

        // The lists are only added together with a new user, an existing user keeps its lists.
        if (user1Id is not null)
        {
            await AddWhiteAndBlackListsForFirstUser(user1Id.Value);
        }

        if (user2Id is not null)
        {
            await AddWhiteAndBlackListsForSecondUser(user2Id.Value);
        }
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
    /// Inserts the <see cref="MqttUser"/>s that don't exist yet.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the identifiers of the inserted users, <c>null</c> for users that already existed.</returns>
    private static async Task<(Guid?, Guid?)> InsertUsers()
    {
        // Add test MQTT user
        var userId = await InsertUserIfMissing(
            new MqttUser
            {
                Id = Guid.NewGuid(),
                UserName = "Test",
                ValidateClientId = false
            },
            "test");

        // Add broker MQTT user
        var user2Id = await InsertUserIfMissing(
            new MqttUser
            {
                Id = Guid.NewGuid(),
                UserName = "mqtt-broker-sync",
                ClientIdPrefix = "mqtt-broker-sync",
                ValidateClientId = true,
                IsSyncUser = true
            },
            "Test");

        return (userId, user2Id);
    }

    /// <summary>
    /// Inserts the <see cref="MqttUser"/> if no user with the same name exists.
    /// </summary>
    /// <param name="mqttUser">The MQTT user.</param>
    /// <param name="password">The password of the MQTT user.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the identifier of the inserted user, <c>null</c> if the user already existed.</returns>
    private static async Task<Guid?> InsertUserIfMissing(MqttUser mqttUser, string password)
    {
        if (await mqttUserRepository.MqttUserNameExists(mqttUser.UserName))
        {
            Console.WriteLine($"The MQTT user {mqttUser.UserName} already exists, keeping it...");
            return null;
        }

        mqttUser.PasswordHash = new PasswordHasher<MqttUser>().HashPassword(mqttUser, password);
        await mqttUserRepository.InsertMqttUser(mqttUser);
        return mqttUser.Id;
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
