// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttUserPropertySurrogate.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Orleans serialization surrogate for the <see cref="MqttUserProperty" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces.Surrogates;

/// <summary>
/// The Orleans serialization surrogate for the <see cref="MqttUserProperty" />.
/// </summary>
[GenerateSerializer]
public struct MqttUserPropertySurrogate
{
    /// <summary>
    /// The name.
    /// </summary>
    [Id(0)]
    public string Name;

    /// <summary>
    /// The value.
    /// </summary>
    [Id(1)]
    public byte[] Value;
}

/// <summary>
/// Converts between <see cref="MqttUserProperty" /> and <see cref="MqttUserPropertySurrogate" />.
/// </summary>
[RegisterConverter]
public sealed class MqttUserPropertySurrogateConverter : IConverter<MqttUserProperty, MqttUserPropertySurrogate>
{
    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttUserProperty ConvertFromSurrogate(in MqttUserPropertySurrogate surrogate)
    {
        return new MqttUserProperty(surrogate.Name, new ReadOnlyMemory<byte>(surrogate.Value ?? []));
    }

    /// <inheritdoc cref="IConverter{TValue, TSurrogate}" />
    public MqttUserPropertySurrogate ConvertToSurrogate(in MqttUserProperty value)
    {
        return new MqttUserPropertySurrogate
        {
            Name = value.Name,
            Value = value.ValueBuffer.ToArray()
        };
    }
}
