// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializationTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that tests the Orleans serialization of the types that are passed to the grains.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces.Tests;

/// <summary>
/// A class that tests the Orleans serialization of the types that are passed to the grains.
/// </summary>
[TestClass]
public class SerializationTests
{
    /// <summary>
    /// The service provider with the Orleans serializer.
    /// </summary>
    private static readonly ServiceProvider ServiceProvider = new ServiceCollection()
        .AddSerializer(
            builder =>
            {
                builder.AddAssembly(typeof(SimpleInterceptingPublishEventArgs).Assembly);
                builder.AddAssembly(typeof(BrokerConnectionSettings).Assembly);
            })
        .BuildServiceProvider();

    /// <summary>
    /// Tests the serialization of the <see cref="SimpleInterceptingPublishEventArgs"/>.
    /// </summary>
    [TestMethod]
    public void TestInterceptingPublishEventArgs()
    {
        var value = new SimpleInterceptingPublishEventArgs
        {
            ApplicationMessage = new MqttApplicationMessage
            {
                Topic = "a/b",
                Payload = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("payload")),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                Retain = true,
                Dup = true,
                ContentType = "text/plain",
                CorrelationData = [1, 2, 3],
                MessageExpiryInterval = 42,
                PayloadFormatIndicator = MqttPayloadFormatIndicator.CharacterData,
                ResponseTopic = "response",
                SubscriptionIdentifiers = [7, 8],
                TopicAlias = 5,
                UserProperties = [new MqttUserProperty("key", new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("value")))]
            },
            ClientId = "client",
            UserName = "user",
            CloseConnection = true,
            ProcessPublish = true,
            SessionItems = new Dictionary<string, object> { ["NotSerializable"] = new() }
        };

        foreach (var result in RoundTrip(value))
        {
            var message = result.ApplicationMessage;
            Assert.AreEqual("a/b", message.Topic);
            Assert.AreEqual("payload", Encoding.UTF8.GetString(message.Payload));
            Assert.AreEqual(MqttQualityOfServiceLevel.ExactlyOnce, message.QualityOfServiceLevel);
            Assert.IsTrue(message.Retain);
            Assert.IsTrue(message.Dup);
            Assert.AreEqual("text/plain", message.ContentType);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, message.CorrelationData);
            Assert.AreEqual(42U, message.MessageExpiryInterval);
            Assert.AreEqual(MqttPayloadFormatIndicator.CharacterData, message.PayloadFormatIndicator);
            Assert.AreEqual("response", message.ResponseTopic);
            CollectionAssert.AreEqual(new List<uint> { 7, 8 }, message.SubscriptionIdentifiers);
            Assert.AreEqual((ushort)5, message.TopicAlias);
            Assert.HasCount(1, message.UserProperties);
            Assert.AreEqual("key", message.UserProperties[0].Name);
            Assert.AreEqual("value", Encoding.UTF8.GetString(message.UserProperties[0].ValueBuffer.Span));
            Assert.AreEqual("client", result.ClientId);
            Assert.AreEqual("user", result.UserName);
            Assert.IsTrue(result.CloseConnection);
            Assert.IsTrue(result.ProcessPublish);
        }
    }

    /// <summary>
    /// Tests the serialization of the <see cref="SimpleInterceptingSubscriptionEventArgs"/>.
    /// </summary>
    [TestMethod]
    public void TestInterceptingSubscriptionEventArgs()
    {
        var value = new SimpleInterceptingSubscriptionEventArgs
        {
            ClientId = "client",
            UserName = "user",
            CloseConnection = true,
            ProcessSubscription = true,
            ReasonString = "reason",
            TopicFilter = new MqttTopicFilter
            {
                Topic = "a/#",
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                NoLocal = true,
                RetainAsPublished = true,
                RetainHandling = MqttRetainHandling.DoNotSendOnSubscribe
            },
            UserProperties = [new MqttUserProperty("key", new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("value")))]
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.AreEqual("client", result.ClientId);
            Assert.AreEqual("user", result.UserName);
            Assert.IsTrue(result.CloseConnection);
            Assert.IsTrue(result.ProcessSubscription);
            Assert.AreEqual("reason", result.ReasonString);
            Assert.AreEqual("a/#", result.TopicFilter.Topic);
            Assert.AreEqual(MqttQualityOfServiceLevel.AtLeastOnce, result.TopicFilter.QualityOfServiceLevel);
            Assert.IsTrue(result.TopicFilter.NoLocal);
            Assert.IsTrue(result.TopicFilter.RetainAsPublished);
            Assert.AreEqual(MqttRetainHandling.DoNotSendOnSubscribe, result.TopicFilter.RetainHandling);
            Assert.HasCount(1, result.UserProperties);
            Assert.AreEqual("key", result.UserProperties[0].Name);
        }
    }

    /// <summary>
    /// Tests the serialization of the <see cref="SimpleValidatingConnectionEventArgs"/>.
    /// </summary>
    [TestMethod]
    public void TestValidatingConnectionEventArgs()
    {
        var value = new SimpleValidatingConnectionEventArgs
        {
            ClientId = "client",
            UserName = "user",
            Password = "password",
            Endpoint = "127.0.0.1:1883",
            CleanSession = true,
            KeepAlivePeriod = 60,
            ProtocolVersion = MqttProtocolVersion.V500,
            ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword,
            AuthenticationData = [4, 5],
            SessionExpiryInterval = 100
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.AreEqual("client", result.ClientId);
            Assert.AreEqual("user", result.UserName);
            Assert.AreEqual("password", result.Password);
            Assert.AreEqual("127.0.0.1:1883", result.Endpoint);
            Assert.IsTrue(result.CleanSession);
            Assert.AreEqual((ushort)60, result.KeepAlivePeriod);
            Assert.AreEqual(MqttProtocolVersion.V500, result.ProtocolVersion);
            Assert.AreEqual(MqttConnectReasonCode.BadUserNameOrPassword, result.ReasonCode);
            CollectionAssert.AreEqual(new byte[] { 4, 5 }, result.AuthenticationData);
            Assert.AreEqual(100U, result.SessionExpiryInterval);
        }
    }

    /// <summary>
    /// Tests the serialization of the <see cref="SimpleClientDisconnectedEventArgs"/>.
    /// </summary>
    [TestMethod]
    public void TestClientDisconnectedEventArgs()
    {
        var value = new SimpleClientDisconnectedEventArgs
        {
            ClientId = "client",
            DisconnectType = MqttClientDisconnectType.Takeover,
            ReasonCode = MqttDisconnectReasonCode.SessionTakenOver
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.AreEqual("client", result.ClientId);
            Assert.AreEqual(MqttClientDisconnectType.Takeover, result.DisconnectType);
            Assert.AreEqual(MqttDisconnectReasonCode.SessionTakenOver, result.ReasonCode);
        }
    }

    /// <summary>
    /// Tests the serialization of the <see cref="SimpleClientUnsubscribedTopicEventArgs"/>.
    /// </summary>
    [TestMethod]
    public void TestClientUnsubscribedTopicEventArgs()
    {
        var value = new SimpleClientUnsubscribedTopicEventArgs
        {
            ClientId = "client",
            UserName = "user",
            TopicFilter = "a/b"
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.AreEqual("client", result.ClientId);
            Assert.AreEqual("user", result.UserName);
            Assert.AreEqual("a/b", result.TopicFilter);
            Assert.IsNotNull(result.SessionItems);
        }
    }

    /// <summary>
    /// Tests the serialization of the <see cref="BrokerConnectionSettings"/> passed as <see cref="IBrokerConnectionSettings"/>.
    /// </summary>
    [TestMethod]
    public void TestBrokerConnectionSettings()
    {
        IBrokerConnectionSettings value = new BrokerConnectionSettings
        {
            ClientId = "sync",
            HostName = "localhost",
            Port = 8883,
            UserName = "user",
            Password = "password",
            UseTls = true,
            UseCleanSession = true
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.IsInstanceOfType<BrokerConnectionSettings>(result);
            Assert.AreEqual("sync", result.ClientId);
            Assert.AreEqual("localhost", result.HostName);
            Assert.AreEqual(8883, result.Port);
            Assert.AreEqual("user", result.UserName);
            Assert.AreEqual("password", result.Password);
            Assert.IsTrue(result.UseTls);
            Assert.IsTrue(result.UseCleanSession);
        }
    }

    /// <summary>
    /// Tests the serialization of the broker list that the repository grain returns and sends to the broker observers.
    /// </summary>
    [TestMethod]
    public void TestBrokerList()
    {
        var brokerId = Guid.NewGuid();
        var value = new Dictionary<Guid, IBrokerConnectionSettings>
        {
            [brokerId] = new BrokerConnectionSettings
            {
                ClientId = "mqtt-broker-sync-2",
                HostName = "localhost",
                Port = 1884,
                UserName = "mqtt-broker-sync",
                Password = "Test"
            }
        };

        foreach (var result in RoundTrip(value))
        {
            Assert.HasCount(1, result);
            var settings = result[brokerId];
            Assert.IsInstanceOfType<BrokerConnectionSettings>(settings);
            Assert.AreEqual("mqtt-broker-sync-2", settings.ClientId);
            Assert.AreEqual("localhost", settings.HostName);
            Assert.AreEqual(1884, settings.Port);
            Assert.AreEqual("mqtt-broker-sync", settings.UserName);
            Assert.AreEqual("Test", settings.Password);
            Assert.IsFalse(settings.UseTls);
            Assert.IsFalse(settings.UseCleanSession);
        }
    }

    /// <summary>
    /// Serializes and deserializes the value and creates a deep copy of it, the same way Orleans does for grain calls.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The deserialized value and the deep copy.</returns>
    private static IEnumerable<T> RoundTrip<T>(T value)
    {
        var serializer = ServiceProvider.GetRequiredService<Serializer>();
        var deserialized = serializer.Deserialize<T>(serializer.SerializeToArray(value));
        Assert.IsNotNull(deserialized);
        yield return deserialized;

        var copier = ServiceProvider.GetRequiredService<DeepCopier>();
        var copy = copier.Copy(value);
        Assert.IsNotNull(copy);
        yield return copy;
    }
}
