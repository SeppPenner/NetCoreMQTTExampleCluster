// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttApplicationMessageSurrogate.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Orleans serialization surrogate for the <see cref="MqttApplicationMessage" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces.Surrogates;

/// <summary>
/// The Orleans serialization surrogate for the <see cref="MqttApplicationMessage" />.
/// </summary>
[GenerateSerializer]
public struct MqttApplicationMessageSurrogate
{
    /// <summary>
    /// The topic.
    /// </summary>
    [Id(0)]
    public string Topic;

    /// <summary>
    /// The payload.
    /// </summary>
    [Id(1)]
    public byte[] Payload;

    /// <summary>
    /// The quality of service level.
    /// </summary>
    [Id(2)]
    public MqttQualityOfServiceLevel QualityOfServiceLevel;

    /// <summary>
    /// A value indicating whether the message is retained or not.
    /// </summary>
    [Id(3)]
    public bool Retain;

    /// <summary>
    /// A value indicating whether the message is a duplicate or not.
    /// </summary>
    [Id(4)]
    public bool Dup;

    /// <summary>
    /// The content type.
    /// </summary>
    [Id(5)]
    public string? ContentType;

    /// <summary>
    /// The correlation data.
    /// </summary>
    [Id(6)]
    public byte[]? CorrelationData;

    /// <summary>
    /// The message expiry interval.
    /// </summary>
    [Id(7)]
    public uint MessageExpiryInterval;

    /// <summary>
    /// The payload format indicator.
    /// </summary>
    [Id(8)]
    public MqttPayloadFormatIndicator PayloadFormatIndicator;

    /// <summary>
    /// The response topic.
    /// </summary>
    [Id(9)]
    public string? ResponseTopic;

    /// <summary>
    /// The subscription identifiers.
    /// </summary>
    [Id(10)]
    public List<uint>? SubscriptionIdentifiers;

    /// <summary>
    /// The topic alias.
    /// </summary>
    [Id(11)]
    public ushort TopicAlias;

    /// <summary>
    /// The user properties.
    /// </summary>
    [Id(12)]
    public List<MqttUserProperty>? UserProperties;
}

/// <summary>
/// Converts between <see cref="MqttApplicationMessage" /> and <see cref="MqttApplicationMessageSurrogate" />.
/// </summary>
[RegisterConverter]
public sealed class MqttApplicationMessageSurrogateConverter : IConverter<MqttApplicationMessage, MqttApplicationMessageSurrogate>
{
    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttApplicationMessage ConvertFromSurrogate(in MqttApplicationMessageSurrogate surrogate)
    {
        return new MqttApplicationMessage
        {
            Topic = surrogate.Topic,
            Payload = new ReadOnlySequence<byte>(surrogate.Payload ?? []),
            QualityOfServiceLevel = surrogate.QualityOfServiceLevel,
            Retain = surrogate.Retain,
            Dup = surrogate.Dup,
            ContentType = surrogate.ContentType,
            CorrelationData = surrogate.CorrelationData,
            MessageExpiryInterval = surrogate.MessageExpiryInterval,
            PayloadFormatIndicator = surrogate.PayloadFormatIndicator,
            ResponseTopic = surrogate.ResponseTopic,
            SubscriptionIdentifiers = surrogate.SubscriptionIdentifiers,
            TopicAlias = surrogate.TopicAlias,
            UserProperties = surrogate.UserProperties
        };
    }

    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttApplicationMessageSurrogate ConvertToSurrogate(in MqttApplicationMessage value)
    {
        return new MqttApplicationMessageSurrogate
        {
            Topic = value.Topic,
            Payload = value.Payload.ToArray(),
            QualityOfServiceLevel = value.QualityOfServiceLevel,
            Retain = value.Retain,
            Dup = value.Dup,
            ContentType = value.ContentType,
            CorrelationData = value.CorrelationData,
            MessageExpiryInterval = value.MessageExpiryInterval,
            PayloadFormatIndicator = value.PayloadFormatIndicator,
            ResponseTopic = value.ResponseTopic,
            SubscriptionIdentifiers = value.SubscriptionIdentifiers,
            TopicAlias = value.TopicAlias,
            UserProperties = value.UserProperties
        };
    }
}
