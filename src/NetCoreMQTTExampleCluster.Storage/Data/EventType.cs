// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventType.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An enumeration representing the event types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data;

/// <summary>
/// An enumeration representing the event types.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    /// <summary>
    /// The subscription event type.
    /// </summary>
    Subscription,

    /// <summary>
    /// The unsubscription event type.
    /// </summary>
    Unsubscription,

    /// <summary>
    /// The connect event type.
    /// </summary>
    Connect,

    /// <summary>
    /// The disconnect event type.
    /// </summary>
    Disconnect,

    /// <summary>
    /// The broker connect event type.
    /// </summary>
    BrokerConnect,

    /// <summary>
    /// The broker disconnect event type.
    /// </summary>
    BrokerDisconnect
}
