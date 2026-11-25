// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleInterceptingSubscriptionEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="InterceptingSubscriptionEventArgs" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="InterceptingSubscriptionEventArgs" />.
/// </summary>
[GenerateSerializer]
public sealed record class SimpleInterceptingSubscriptionEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleInterceptingSubscriptionEventArgs"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleInterceptingSubscriptionEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleInterceptingSubscriptionEventArgs"/> class.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    public SimpleInterceptingSubscriptionEventArgs(InterceptingSubscriptionEventArgs eventArgs)
    {
        this.ClientId = eventArgs.ClientId;
        this.UserName = eventArgs.UserName;
        this.CloseConnection = eventArgs.CloseConnection;
        this.ProcessSubscription = eventArgs.ProcessSubscription;
        this.ReasonString = eventArgs.ReasonString;
        this.Response = eventArgs.Response;
        this.SessionItems = eventArgs.SessionItems;
        this.TopicFilter = eventArgs.TopicFilter;
        this.UserProperties = eventArgs.UserProperties;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [Id(0)]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [Id(1)]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the connection will be closed or not.
    /// </summary>
    [Id(2)]
    public bool CloseConnection { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the subscribe command is accepted or not.
    /// </summary>
    [Id(3)]
    public bool ProcessSubscription { get; init; }

    /// <summary>
    /// Gets or sets the reason string.
    /// </summary>
    [Id(4)]
    public string ReasonString { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the response.
    /// </summary>
    /// <remarks>Not transferred to the grains.</remarks>
    [field: NonSerialized]
    public SubscribeResponse Response { get; init; } = new();

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    /// <remarks>Not transferred to the grains.</remarks>
    [field: NonSerialized]
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the topic filter.
    /// </summary>
    [Id(5)]
    public MqttTopicFilter TopicFilter { get; init; } = new();

    /// <summary>
    /// Gets or sets the user properties.
    /// </summary>
    [Id(6)]
    public List<MqttUserProperty> UserProperties { get; init; } = [];
}
