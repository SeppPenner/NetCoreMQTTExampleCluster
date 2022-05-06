// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMqttSubscriptionInterceptorContext.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="MqttSubscriptionInterceptorContext" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="MqttSubscriptionInterceptorContext" />.
/// </summary>
public class SimpleMqttSubscriptionInterceptorContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleMqttSubscriptionInterceptorContext"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleMqttSubscriptionInterceptorContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleMqttSubscriptionInterceptorContext"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public SimpleMqttSubscriptionInterceptorContext(MqttSubscriptionInterceptorContext context)
    {
        this.ClientId = context.ClientId;
        this.TopicFilter = context.TopicFilter;
        this.SessionItems = context.SessionItems;
        this.AcceptSubscription = context.AcceptSubscription;
        this.CloseConnection = context.CloseConnection;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the topic filter.
    /// </summary>
    public MqttTopicFilter TopicFilter { get; set; } = new();

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    public IDictionary<object, object> SessionItems { get; set; } = new Dictionary<object, object>();
    
    /// <summary>
    /// Gets or sets a value indicating whether the subscribe command is accepted or not.
    /// </summary>
    public bool AcceptSubscription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection will be closed or not.
    /// </summary>
    public bool CloseConnection { get; set; }
}
