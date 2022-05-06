// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMqttApplicationMessageInterceptorContext.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="MqttApplicationMessageInterceptorContext" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

/// <summary>
/// A class that contains a simplified version of the <see cref="MqttApplicationMessageInterceptorContext" />.
/// </summary>
public class SimpleMqttApplicationMessageInterceptorContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleMqttApplicationMessageInterceptorContext"/> class. This is used for testing purposes only!
    /// </summary>
    public SimpleMqttApplicationMessageInterceptorContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleMqttApplicationMessageInterceptorContext"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public SimpleMqttApplicationMessageInterceptorContext(MqttApplicationMessageInterceptorContext context)
    {
        this.ClientId = context.ClientId;
        this.ApplicationMessage = context.ApplicationMessage;
        this.SessionItems = context.SessionItems;
        this.AcceptPublish = context.AcceptPublish;
        this.CloseConnection = context.CloseConnection;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application message.
    /// </summary>
    public MqttApplicationMessage ApplicationMessage { get; set; } = new();

    /// <summary>
    /// Gets or sets the session items.
    /// </summary>
    public IDictionary<object, object> SessionItems { get; set; } = new Dictionary<object, object>();
    
    /// <summary>
    /// Gets or sets a value indicating whether the publish command is accepted or not.
    /// </summary>
    public bool AcceptPublish { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection will be closed or not.
    /// </summary>
    public bool CloseConnection { get; set; }
}
