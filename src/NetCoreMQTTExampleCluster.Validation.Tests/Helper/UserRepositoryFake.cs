// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepositoryFake.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A fake implementation supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation.Tests.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

    /// <inheritdoc cref="IUserRepository" />
    /// <summary>
    ///      A fake implementation supporting the repository pattern to work with <see cref="User" />s.
    /// </summary>
    /// <seealso cref="IUserRepository" />
    public class UserRepositoryFake : IUserRepository
    {
        /// <summary>
        /// The users.
        /// </summary>
        private readonly List<User> users;

        /// <summary>
        /// The white lists.
        /// </summary>
        private readonly Dictionary<Guid, List<BlacklistWhitelist>> whiteLists;

        /// <summary>
        /// The blacklists.
        /// </summary>
        private readonly Dictionary<Guid, List<BlacklistWhitelist>> blacklists;

        /// <summary>
        /// The password hasher.
        /// </summary>
        private readonly IPasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepositoryFake"/> class.
        /// </summary>
        /// <param name="user1Id">The user identifier for user 1.</param>
        public UserRepositoryFake(Guid user1Id)
        {
            this.users = new List<User>
            {
                new User
                {
                    Id = user1Id,
                    UserName = "Test",
                    ValidateClientId = false
                }
            };

            this.blacklists = new Dictionary<Guid, List<BlacklistWhitelist>>
            {
                {
                    user1Id,
                    new List<BlacklistWhitelist>()
                }
            };

            this.whiteLists = new Dictionary<Guid, List<BlacklistWhitelist>>
            {
                {
                    user1Id,
                    new List<BlacklistWhitelist>
                    {
                        new BlacklistWhitelist
                        {
                            Type = BlacklistWhitelistType.Publish,
                            UserId = user1Id,
                            Value = "a/b"
                        },
                        new BlacklistWhitelist
                        {
                            Type = BlacklistWhitelistType.Publish,
                            UserId = user1Id,
                            Value = "a/d"
                        },
                        new BlacklistWhitelist
                        {
                            Type = BlacklistWhitelistType.Subscribe,
                            UserId = user1Id,
                            Value = "d/e"
                        },
                        new BlacklistWhitelist
                        {
                            Type = BlacklistWhitelistType.Subscribe,
                            UserId = user1Id,
                            Value = "e"
                        }
                    }
                }
            };

            this.users.ElementAt(0).PasswordHash = this.passwordHasher.HashPassword(this.users.ElementAt(0), "test");
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<User>> GetUsers()
        {
            await Task.Delay(1);
            return this.users;
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="User" /> by their identifier.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s identifier to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<User> GetUserById(Guid userId)
        {
            await Task.Delay(1);
            return this.users.FirstOrDefault(u => u.Id == userId);
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="User" /> by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<User> GetUserByName(string userName)
        {
            await Task.Delay(1);
            return this.users.FirstOrDefault(u => u.UserName == userName);
        }

        /// <summary>
        ///     Gets a <see cref="User" />'s name and identifier by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public async Task<(string, Guid)> GetUserNameAndUserIdByName(string userName)
        {
            await Task.Delay(1);
            var user = this.users.FirstOrDefault(u => u.UserName == userName);
            return user == null ? (null, Guid.Empty) : (user.UserName, user.Id);
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets a <see cref="bool" /> value indicating whether the user name already exists or not.
        /// </summary>
        /// <param name="userName">The user name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<bool> UserNameExists(string userName)
        {
            await Task.Delay(1);
            return this.users.Any(u => u.UserName == userName);
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Inserts a <see cref="User" /> to the database.
        /// </summary>
        /// <param name="user">The <see cref="User" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<bool> InsertUser(User user)
        {
            await Task.Delay(1);
            this.users.Add(user);
            return true;
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the blacklist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type)
        {
            await Task.Delay(1);
            return this.blacklists.ContainsKey(userId) ? this.blacklists[userId].Where(b => b.Type == type).ToList() : new List<BlacklistWhitelist>();
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the whitelist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type)
        {
            await Task.Delay(1);
            return this.whiteLists.ContainsKey(userId) ? this.whiteLists[userId].Where(b => b.Type == type).ToList() : new List<BlacklistWhitelist>();
        }

        /// <inheritdoc cref="IUserRepository" />
        /// <summary>
        ///     Gets the client identifier prefixes for all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IUserRepository" />
        public async Task<List<string>> GetAllClientIdPrefixes()
        {
            await Task.Delay(1);
            return this.users.Select(u => u.ClientIdPrefix ?? string.Empty).ToList();
        }
    }
}
