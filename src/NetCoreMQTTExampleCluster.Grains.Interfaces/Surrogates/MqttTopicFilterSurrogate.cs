// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttTopicFilterSurrogate.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Orleans serialization surrogate for the <see cref="MqttTopicFilter" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces.Surrogates;

/// <summary>
/// The Orleans serialization surrogate for the <see cref="MqttTopicFilter" />.
/// </summary>
[GenerateSerializer]
public struct MqttTopicFilterSurrogate
{
    /// <summary>
    /// The topic.
    /// </summary>
    [Id(0)]
    public string Topic;

    /// <summary>
    /// The quality of service level.
    /// </summary>
    [Id(1)]
    public MqttQualityOfServiceLevel QualityOfServiceLevel;

    /// <summary>
    /// A value indicating whether the no local option is set or not.
    /// </summary>
    [Id(2)]
    public bool NoLocal;

    /// <summary>
    /// A value indicating whether the retain as published option is set or not.
    /// </summary>
    [Id(3)]
    public bool RetainAsPublished;

    /// <summary>
    /// The retain handling.
    /// </summary>
    [Id(4)]
    public MqttRetainHandling RetainHandling;
}

/// <summary>
/// Converts between <see cref="MqttTopicFilter" /> and <see cref="MqttTopicFilterSurrogate" />.
/// </summary>
[RegisterConverter]
public sealed class MqttTopicFilterSurrogateConverter : IConverter<MqttTopicFilter, MqttTopicFilterSurrogate>
{
    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttTopicFilter ConvertFromSurrogate(in MqttTopicFilterSurrogate surrogate)
    {
        return new MqttTopicFilter
        {
            Topic = surrogate.Topic,
            QualityOfServiceLevel = surrogate.QualityOfServiceLevel,
            NoLocal = surrogate.NoLocal,
            RetainAsPublished = surrogate.RetainAsPublished,
            RetainHandling = surrogate.RetainHandling
        };
    }

    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttTopicFilterSurrogate ConvertToSurrogate(in MqttTopicFilter value)
    {
        return new MqttTopicFilterSurrogate
        {
            Topic = value.Topic,
            QualityOfServiceLevel = value.QualityOfServiceLevel,
            NoLocal = value.NoLocal,
            RetainAsPublished = value.RetainAsPublished,
            RetainHandling = value.RetainHandling
        };
    }
}
