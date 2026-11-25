// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleInterceptingPublishEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="InterceptingPublishEventArgs" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="InterceptingPublishEventArgs" />.
/// </summary>
[GenerateSerializer]
public sealed record class SimpleInterceptingPublishEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleInterceptingPublishEventArgs"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleInterceptingPublishEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleInterceptingPublishEventArgs"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public SimpleInterceptingPublishEventArgs(InterceptingPublishEventArgs context)
    {
        this.ApplicationMessage = context.ApplicationMessage;
        this.ClientId = context.ClientId;
        this.UserName = context.UserName;
        this.CloseConnection = context.CloseConnection;
        this.ProcessPublish = context.ProcessPublish;
        this.Response = context.Response;
        this.SessionItems = context.SessionItems;
    }

    /// <summary>
    /// Gets or sets the application message.
    /// </summary>
    [Id(0)]
    public MqttApplicationMessage ApplicationMessage { get; init; } = new();

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [Id(1)]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Id(2)]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the connection will be closed or not.
    /// </summary>
    [Id(3)]
    public bool CloseConnection { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the publish command is accepted or not.
    /// </summary>
    [Id(4)]
    public bool ProcessPublish { get; init; }

    /// <summary>
    /// Gets or sets the response.
    /// </summary>
    /// <remarks>Not transferred to the grains.</remarks>
    [field: NonSerialized]
    public PublishResponse Response { get; } = new();

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    /// <remarks>Not transferred to the grains.</remarks>
    [field: NonSerialized]
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();
}
