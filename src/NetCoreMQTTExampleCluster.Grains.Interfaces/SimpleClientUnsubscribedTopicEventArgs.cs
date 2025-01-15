// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleClientUnsubscribedTopicEventArgs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="ClientUnsubscribedTopicEventArgs" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="ClientUnsubscribedTopicEventArgs" />.
/// </summary>
public sealed record class SimpleClientUnsubscribedTopicEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleClientUnsubscribedTopicEventArgs"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleClientUnsubscribedTopicEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleClientUnsubscribedTopicEventArgs"/> class.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    public SimpleClientUnsubscribedTopicEventArgs(ClientUnsubscribedTopicEventArgs eventArgs)
    {
        this.ClientId = eventArgs.ClientId;
        this.SessionItems = eventArgs.SessionItems;
        this.UserName = eventArgs.UserName;
        this.TopicFilter = eventArgs.TopicFilter;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    public IDictionary SessionItems { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the topic filter.
    /// </summary>
    public string TopicFilter { get; init; } = string.Empty;
}
