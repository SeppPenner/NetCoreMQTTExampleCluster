// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleClientDisconnectedEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="ClientDisconnectedEventArgs" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="ClientDisconnectedEventArgs" />.
/// </summary>
public sealed record class SimpleClientDisconnectedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleClientDisconnectedEventArgs"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleClientDisconnectedEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleClientDisconnectedEventArgs"/> class.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    public SimpleClientDisconnectedEventArgs(ClientDisconnectedEventArgs eventArgs)
    {
        this.ClientId = eventArgs.ClientId;
        this.UserName = eventArgs.UserName;
        this.Password = eventArgs.Password;
        this.DisconnectType = eventArgs.DisconnectType;
        this.Endpoint = eventArgs.RemoteEndPoint.ToString() ?? string.Empty;
        this.ReasonCode = eventArgs.ReasonCode;
        this.ReasonString = eventArgs.ReasonString;
        this.SessionExpiryInterval = eventArgs.SessionExpiryInterval;
        this.SessionItems = eventArgs.SessionItems;
        this.UserProperties = eventArgs.UserProperties;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the disconnect type.
    /// </summary>
    public MqttClientDisconnectType DisconnectType { get; init; }

    /// <summary>
    /// Gets or sets the endpoint.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason code.
    /// </summary>
    public MqttDisconnectReasonCode? ReasonCode { get; init; }

    /// <summary>
    /// Gets or sets the reason string.
    /// </summary>
    public string ReasonString { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the session expiry interval.
    /// </summary>
    public uint SessionExpiryInterval { get; init; }

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the user properties.
    /// </summary>
    public List<MqttUserProperty> UserProperties { get; init; } = [];
}
