// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleValidatingConnectionEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="ValidatingConnectionEventArgs" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="ValidatingConnectionEventArgs" />.
/// </summary>
public sealed record class SimpleValidatingConnectionEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleValidatingConnectionEventArgs"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleValidatingConnectionEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleValidatingConnectionEventArgs"/> class.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    public SimpleValidatingConnectionEventArgs(ValidatingConnectionEventArgs eventArgs)
    {
        this.AssignedClientIdentifier = eventArgs.AssignedClientIdentifier;
        this.AuthenticationData = eventArgs.AuthenticationData;
        this.AuthenticationMethod = eventArgs.AuthenticationMethod;
        this.CleanSession = eventArgs.CleanSession;
        this.ClientId = eventArgs.ClientId;
        this.IsSecureConnection = eventArgs.IsSecureConnection;
        this.KeepAlivePeriod = eventArgs.KeepAlivePeriod;
        this.MaximumPacketSize = eventArgs.MaximumPacketSize;
        this.Password = eventArgs.Password;
        this.ProtocolVersion = eventArgs.ProtocolVersion;
        this.ReasonCode = eventArgs.ReasonCode;
        this.ReasonString = eventArgs.ReasonString;
        this.ReceiveMaximum = eventArgs.ReceiveMaximum;
        this.Endpoint = eventArgs.RemoteEndPoint.ToString() ?? string.Empty;
        this.RequestProblemInformation = eventArgs.RequestProblemInformation;
        this.RequestResponseInformation = eventArgs.RequestResponseInformation;
        this.ResponseAuthenticationData = eventArgs.ResponseAuthenticationData;
        this.ResponseUserProperties = eventArgs.ResponseUserProperties;
        this.ServerReference = eventArgs.ServerReference;
        this.SessionExpiryInterval = eventArgs.SessionExpiryInterval;
        this.SessionItems = eventArgs.SessionItems;
        this.TopicAliasMaximum = eventArgs.TopicAliasMaximum;
        this.UserName = eventArgs.UserName;
        this.UserProperties = eventArgs.UserProperties;
        this.WillDelayInterval = eventArgs.WillDelayInterval;
    }

    /// <summary>
    /// Gets or sets the assigned client identifier.
    /// </summary>
    public string AssignedClientIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication data.
    /// </summary>
    public byte[] AuthenticationData { get; init; } = [];

    /// <summary>
    /// Gets or sets the authentication method.
    /// </summary>
    public string AuthenticationMethod { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a clean session is used or not.
    /// </summary>
    public bool? CleanSession { get; init; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the connection is secure or not.
    /// </summary>
    public bool IsSecureConnection { get; init; }

    /// <summary>
    /// Gets or sets the keepalive period.
    /// </summary>
    public ushort? KeepAlivePeriod { get; init; }

    /// <summary>
    /// Gets or sets the maximum packet size.
    /// </summary>
    public uint MaximumPacketSize { get; init; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the protocol version.
    /// </summary>
    public MqttProtocolVersion ProtocolVersion { get; init; }

    /// <summary>
    /// Gets or sets the reason code.
    /// </summary>
    public MqttConnectReasonCode ReasonCode { get; init; }

    /// <summary>
    /// Gets or sets the reason string.
    /// </summary>
    public string ReasonString { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the receive maximum.
    /// </summary>
    public ushort ReceiveMaximum { get; init; }

    /// <summary>
    /// Gets or sets the endpoint.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request problem information is set or not.
    /// </summary>
    public bool RequestProblemInformation { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the request response information is set or not.
    /// </summary>
    public bool RequestResponseInformation { get; init; }

    /// <summary>
    /// Gets or sets the response authentication data.
    /// </summary>
    public byte[] ResponseAuthenticationData { get; init; } = [];

    /// <summary>
    /// Gets or sets the response user properties.
    /// </summary>
    public List<MqttUserProperty> ResponseUserProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets the server reference.
    /// </summary>
    public string ServerReference { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the session expiry interval.
    /// </summary>
    public uint SessionExpiryInterval { get; init; }

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the topic alias maximum.
    /// </summary>
    public ushort TopicAliasMaximum { get; init; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user properties.
    /// </summary>
    public List<MqttUserProperty> UserProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets the will delay interval.
    /// </summary>
    public uint WillDelayInterval { get; init; }
}
