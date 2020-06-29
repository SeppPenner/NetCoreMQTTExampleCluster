// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestValidation.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that tests the <see cref="MqttValidator"/>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MQTTnet;
    using MQTTnet.Protocol;
    using MQTTnet.Server;

    using NetCoreMQTTExampleCluster.Grains.Interfaces;
    using NetCoreMQTTExampleCluster.Storage.Data;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Validation.Tests.Helper;

    /// <summary>
    /// A class that tests the <see cref="MqttValidator"/>.
    /// </summary>
    [TestClass]
    public class TestValidation
    {
        /// <summary>
        /// The identifier for the Test user.
        /// </summary>
        private static readonly Guid User1Id = Guid.NewGuid();

        /// <summary>
        /// The MQTT validator.
        /// </summary>
        private readonly IMqttValidator mqttValidator = new MqttValidator();

        /// <summary>
        /// The password hasher.
        /// </summary>
        private readonly IPasswordHasher<User> passwordHasher = new PasswordHasher<User>();

        /// <summary>
        /// The user repository.
        /// </summary>
        private readonly IUserRepository userRepository = new UserRepositoryFake(User1Id);

        /// <summary>
        /// Tests the validate connection method of the <see cref="MqttValidator"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task TestValidateConnection()
        {
            // Test user
            var mqttConnectionValidatorContext = new SimpleMqttConnectionValidatorContext
            {
                ClientId = "Test",
                CleanSession = true,
                Endpoint = "127.0.0.1",
                Password = "test",
                UserName = "Test"
            };

            var result = await this.mqttValidator.ValidateConnection(
                mqttConnectionValidatorContext,
                this.userRepository,
                new Dictionary<string, User>(), 
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
            var users = new Dictionary<string, User>
            {
                {
                    "Test",
                    new User
                    {
                        Id = User1Id,
                        UserName = "Test",
                        ValidateClientId = false
                    }
                }
            };

            // Test user
            var mqttApplicationMessage = new MqttApplicationMessage
            {
                Retain = false,
                Payload = Encoding.UTF8.GetBytes("asdf"),
                Topic = "a/b",
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            };

            var mqttConnectionValidatorContext = new MqttApplicationMessageInterceptorContext("Test", new Dictionary<object, object>(), mqttApplicationMessage);

            var result = await this.mqttValidator.ValidatePublish(
                             mqttConnectionValidatorContext,
                             this.userRepository,
                             users,
                             dataLimitCacheMonth);

            Assert.IsTrue(result);

            mqttApplicationMessage = new MqttApplicationMessage
            {
                Retain = false,
                Payload = Encoding.UTF8.GetBytes("asdf"),
                Topic = "a/d",
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            };

            mqttConnectionValidatorContext = new MqttApplicationMessageInterceptorContext("Test", new Dictionary<object, object>(), mqttApplicationMessage);

            result = await this.mqttValidator.ValidatePublish(
                             mqttConnectionValidatorContext,
                             this.userRepository,
                             users,
                             dataLimitCacheMonth);

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
            var users = new Dictionary<string, User>
            {
                {
                    "Test",
                    new User
                    {
                        Id = User1Id,
                        UserName = "Test",
                        ValidateClientId = false
                    }
                }
            };

            // Test user
            var mqttTopicFilter = new MqttTopicFilter
            {
                Topic = "d/e",
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            };

            var mqttConnectionValidatorContext = new MqttSubscriptionInterceptorContext("Test", mqttTopicFilter, new Dictionary<object, object>());

            var result = await this.mqttValidator.ValidateSubscription(
                             mqttConnectionValidatorContext,
                             this.userRepository,
                             users);

            Assert.IsTrue(result);

            mqttTopicFilter = new MqttTopicFilter
            {
                Topic = "e",
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
            };

            mqttConnectionValidatorContext = new MqttSubscriptionInterceptorContext("Test", mqttTopicFilter, new Dictionary<object, object>());

            result = await this.mqttValidator.ValidateSubscription(
                             mqttConnectionValidatorContext,
                             this.userRepository,
                             users);

            Assert.IsTrue(result);
        }
    }
}
