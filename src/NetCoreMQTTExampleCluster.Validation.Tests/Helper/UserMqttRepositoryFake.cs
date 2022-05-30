// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRepositoryFake.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A fake implementation supporting the repository pattern to work with <see cref="User" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation.Tests.Helper;

/// <inheritdoc cref="IMqttUserRepository" />
public class UserMqttRepositoryFake : IMqttUserRepository
{
    /// <summary>
    /// The users.
    /// </summary>
    private readonly List<MqttUser> mqttUsers;

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
    private readonly IPasswordHasher<MqttUser> passwordHasher = new PasswordHasher<MqttUser>();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserMqttRepositoryFake"/> class.
    /// </summary>
    /// <param name="user1Id">The user identifier for MQTT user 1.</param>
    public UserMqttRepositoryFake(Guid user1Id)
    {
        this.mqttUsers = new List<MqttUser>
        {
            new MqttUser
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

        this.mqttUsers.ElementAt(0).PasswordHash = this.passwordHasher.HashPassword(this.mqttUsers.ElementAt(0), "test");
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<MqttUser>> GetUsers()
    {
        await Task.Delay(1);
        return this.mqttUsers;
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUser> GetUserById(Guid userId)
    {
        await Task.Delay(1);
        return this.mqttUsers.FirstOrDefault(u => u.Id == userId);
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUser> GetUserByName(string userName)
    {
        await Task.Delay(1);
        return this.mqttUsers.FirstOrDefault(u => u.UserName == userName);
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<(string, Guid)> GetUserNameAndUserIdByName(string userName)
    {
        await Task.Delay(1);
        var mqttUser = this.mqttUsers.FirstOrDefault(u => u.UserName == userName);
        return mqttUser is null ? (null, Guid.Empty) : (mqttUser.UserName, mqttUser.Id);
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<bool> UserNameExists(string userName)
    {
        await Task.Delay(1);
        return this.mqttUsers.Any(u => u.UserName == userName);
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<bool> InsertUser(MqttUser mqttUser)
    {
        await Task.Delay(1);
        this.mqttUsers.Add(mqttUser);
        return true;
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetBlacklistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await Task.Delay(1);
        return this.blacklists.ContainsKey(userId) ? this.blacklists[userId].Where(b => b.Type == type).ToList() : new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<BlacklistWhitelist>> GetWhitelistItemsForUser(Guid userId, BlacklistWhitelistType type)
    {
        await Task.Delay(1);
        return this.whiteLists.ContainsKey(userId) ? this.whiteLists[userId].Where(b => b.Type == type).ToList() : new List<BlacklistWhitelist>();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<List<string>> GetAllClientIdPrefixes()
    {
        await Task.Delay(1);
        return this.mqttUsers.Select(u => u.ClientIdPrefix ?? string.Empty).ToList();
    }

    /// <inheritdoc cref="IMqttUserRepository" />
    public async Task<MqttUserData> GetUserData(Guid userId)
    {
        await Task.Delay(1);
        return new MqttUserData();
    }
}
