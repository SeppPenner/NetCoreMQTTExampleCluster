// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestValidation.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that tests the <see cref="MqttValidator"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation.Tests;

using System.Buffers;

/// <summary>
/// A class that tests the <see cref="MqttValidator"/>.
/// </summary>
[TestClass]
public class TestValidation
{
    /// <summary>
    /// The identifier for MQTT user 1.
    /// </summary>
    private static readonly Guid User1Id = Guid.NewGuid();

    /// <summary>
    /// The client identifier prefixes.
    /// </summary>
    private static readonly List<string> ClientIdPrefixes = new() { "Test" };

    /// <summary>
    /// The MQTT validator.
    /// </summary>
    private readonly IMqttValidator mqttValidator = new MqttValidator();

    /// <summary>
    /// The password hasher.
    /// </summary>
    private readonly IPasswordHasher<MqttUser> passwordHasher = new PasswordHasher<MqttUser>();

    /// <summary>
    /// The MQTT user repository.
    /// </summary>
    private readonly IMqttUserRepository mqttUserRepository = new UserMqttRepositoryFake(User1Id);

    /// <summary>
    /// Tests the validate connection method of the <see cref="MqttValidator"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestValidateConnection()
    {
        // Test MQTT user
        var mqttConnectionValidatorContext = new SimpleValidatingConnectionEventArgs
        {
            ClientId = "Test",
            CleanSession = true,
            Endpoint = "127.0.0.1",
            Password = "test",
            UserName = "Test"
        };

        var mqttUser = await this.mqttUserRepository.GetMqttUserByName("Test");

        var result = this.mqttValidator.ValidateConnection(
            mqttConnectionValidatorContext,
            mqttUser,
            this.passwordHasher);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests the validate publish method of the <see cref="MqttValidator"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestValidatePublish()
    {
        var dataLimitCacheMonth = new MemoryCache(new MemoryCacheOptions());

        // Add users to users dictionary to simulate that the connection was established successfully:
        var users = new Dictionary<string, MqttUser>
        {
            {
                "Test",
                new MqttUser
                {
                    Id = User1Id,
                    UserName = "Test",
                    ValidateClientId = false
                }
            }
        };

        // Test data.
        var byteData = Encoding.UTF8.GetBytes("asdf");
        var memory = new ReadOnlyMemory<byte>(byteData);
        var sequence = new ReadOnlySequence<byte>(memory);

        var mqttApplicationMessage = new MqttApplicationMessage
        {
            Retain = false,
            Payload = sequence,
            Topic = "a/b",
            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
        };

        var mqttConnectionValidatorContext = new SimpleInterceptingPublishEventArgs()
        {
            ApplicationMessage = mqttApplicationMessage,
            ClientId = "Test"
        };

        var blacklist = await this.mqttUserRepository.GetBlacklistItemsForMqttUser(User1Id, BlacklistWhitelistType.Publish);
        var whitelist = await this.mqttUserRepository.GetWhitelistItemsForMqttUser(User1Id, BlacklistWhitelistType.Publish);
        var result = this.mqttValidator.ValidatePublish(
            mqttConnectionValidatorContext,
            blacklist,
            whitelist,
            users["Test"],
            dataLimitCacheMonth,
            ClientIdPrefixes);

        Assert.IsTrue(result);

        mqttApplicationMessage = new MqttApplicationMessage
        {
            Retain = false,
            Payload = sequence,
            Topic = "a/d",
            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
        };

        mqttConnectionValidatorContext = new SimpleInterceptingPublishEventArgs()
        {
            ApplicationMessage = mqttApplicationMessage,
            ClientId = "Test"
        };

        result = this.mqttValidator.ValidatePublish(
            mqttConnectionValidatorContext,
            blacklist,
            whitelist,
            users["Test"],
            dataLimitCacheMonth,
            ClientIdPrefixes);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests the validate subscription method of the <see cref="MqttValidator"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestValidateSubscription()
    {
        // Add users to users dictionary to simulate that the connection was established successfully:
        var users = new Dictionary<string, MqttUser>
        {
            {
                "Test",
                new MqttUser
                {
                    Id = User1Id,
                    UserName = "Test",
                    ValidateClientId = false
                }
            }
        };

        // Test data
        var mqttTopicFilter = new MqttTopicFilter
        {
            Topic = "d/e",
            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
        };

        var mqttConnectionValidatorContext = new SimpleInterceptingSubscriptionEventArgs()
        {
            ClientId = "Test",
            TopicFilter = mqttTopicFilter
        };

        var blacklist = await this.mqttUserRepository.GetBlacklistItemsForMqttUser(User1Id, BlacklistWhitelistType.Subscribe);
        var whitelist = await this.mqttUserRepository.GetWhitelistItemsForMqttUser(User1Id, BlacklistWhitelistType.Subscribe);
        var result = this.mqttValidator.ValidateSubscription(
            mqttConnectionValidatorContext,
            blacklist,
            whitelist,
            users["Test"],
            ClientIdPrefixes);

        Assert.IsTrue(result);

        mqttTopicFilter = new MqttTopicFilter
        {
            Topic = "e",
            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
        };

        mqttConnectionValidatorContext = new SimpleInterceptingSubscriptionEventArgs()
        {
            ClientId = "Test",
            TopicFilter = mqttTopicFilter
        };

        result = this.mqttValidator.ValidateSubscription(
            mqttConnectionValidatorContext,
            blacklist,
            whitelist,
            users["Test"],
            ClientIdPrefixes);

        Assert.IsTrue(result);
    }
}
