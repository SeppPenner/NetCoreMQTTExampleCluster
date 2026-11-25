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
[GenerateSerializer]
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
    [Id(0)]
    public string AssignedClientIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication data.
    /// </summary>
    [Id(1)]
    public byte[] AuthenticationData { get; init; } = [];

    /// <summary>
    /// Gets or sets the authentication method.
    /// </summary>
    [Id(2)]
    public string AuthenticationMethod { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a clean session is used or not.
    /// </summary>
    [Id(3)]
    public bool? CleanSession { get; init; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [Id(4)]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the connection is secure or not.
    /// </summary>
    [Id(5)]
    public bool IsSecureConnection { get; init; }

    /// <summary>
    /// Gets or sets the keepalive period.
    /// </summary>
    [Id(6)]
    public ushort? KeepAlivePeriod { get; init; }

    /// <summary>
    /// Gets or sets the maximum packet size.
    /// </summary>
    [Id(7)]
    public uint MaximumPacketSize { get; init; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Id(8)]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the protocol version.
    /// </summary>
    [Id(9)]
    public MqttProtocolVersion ProtocolVersion { get; init; }

    /// <summary>
    /// Gets or sets the reason code.
    /// </summary>
    [Id(10)]
    public MqttConnectReasonCode ReasonCode { get; init; }

    /// <summary>
    /// Gets or sets the reason string.
    /// </summary>
    [Id(11)]
    public string ReasonString { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the receive maximum.
    /// </summary>
    [Id(12)]
    public ushort ReceiveMaximum { get; init; }

    /// <summary>
    /// Gets or sets the endpoint.
    /// </summary>
    [Id(13)]
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request problem information is set or not.
    /// </summary>
    [Id(14)]
    public bool RequestProblemInformation { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the request response information is set or not.
    /// </summary>
    [Id(15)]
    public bool RequestResponseInformation { get; init; }

    /// <summary>
    /// Gets or sets the response authentication data.
    /// </summary>
    [Id(16)]
    public byte[] ResponseAuthenticationData { get; init; } = [];

    /// <summary>
    /// Gets or sets the response user properties.
    /// </summary>
    [Id(17)]
    public List<MqttUserProperty> ResponseUserProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets the server reference.
    /// </summary>
    [Id(18)]
    public string ServerReference { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the session expiry interval.
    /// </summary>
    [Id(19)]
    public uint SessionExpiryInterval { get; init; }

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    /// <remarks>Not transferred to the grains.</remarks>
    [field: NonSerialized]
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the topic alias maximum.
    /// </summary>
    [Id(20)]
    public ushort TopicAliasMaximum { get; init; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Id(21)]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user properties.
    /// </summary>
    [Id(22)]
    public List<MqttUserProperty> UserProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets the will delay interval.
    /// </summary>
    [Id(23)]
    public uint WillDelayInterval { get; init; }
}
