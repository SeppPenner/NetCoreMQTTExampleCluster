// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A program to setup the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.DatabaseSetup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    using NetCoreMQTTExampleCluster.Storage;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    using Newtonsoft.Json;

    /// <summary>
    ///     A program to setup the database.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The database version repository.
        /// </summary>
        private static IDatabaseVersionRepository databaseVersionRepository;

        /// <summary>
        /// The whitelist repository.
        /// </summary>
        private static WhitelistRepository whitelistRepository;

        /// <summary>
        ///     The user repository.
        /// </summary>
        private static IUserRepository userRepository;

        /// <summary>
        ///     The database helper.
        /// </summary>
        private static IDatabaseHelper databaseHelper;

        /// <summary>
        ///     The main method of the program.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public static async Task Main()
        {
            var connectionSettings = await ReadSettingsFile();

            userRepository = new UserRepository(connectionSettings);
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
            // ReSharper disable once AssignNullToNotNullAttribute
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
        /// Adds the blacklist and whitelist items for the first user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        /// Inserts the <see cref="User"/>s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        private static async Task<(Guid, Guid)> InsertUsers()
        {
            var passwordHasher = new PasswordHasher<User>();

            // Add test user
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                UserName = "Test",
                ValidateClientId = false
            };

            user.PasswordHash = passwordHasher.HashPassword(user, "test");
            await userRepository.InsertUser(user);

            // Add broker user
            var user2Id = Guid.NewGuid();

            var user2 = new User
            {
                Id = user2Id,
                UserName = "mqtt-broker-sync",
                ClientIdPrefix = "mqtt-broker-sync",
                ValidateClientId = true,
                IsSyncUser = true
            };
            user2.PasswordHash = passwordHasher.HashPassword(user2, "Test");

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
}
