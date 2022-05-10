// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventLog.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The event log class. It contains information about events that occurred on the other database tables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data;

/// <summary>
/// The event log class. It contains information about events that occurred on the other database tables.
/// </summary>
public class EventLog
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the event details.
    /// </summary>
    public string EventDetails { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created at timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Returns a <see cref="string"></see> representation of the <see cref="EventLog"/> class.
    /// </summary>
    /// <returns>A <see cref="string"></see> representation of the <see cref="EventLog"/> class.</returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
