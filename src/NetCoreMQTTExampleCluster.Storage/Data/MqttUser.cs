// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttUser.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The MQTT user class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data;

/// <summary>
/// The MQTT user class.
/// </summary>
public class MqttUser
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a salted and hashed representation of the password.
    /// </summary>
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier prefix.
    /// </summary>
    public string ClientIdPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the client identifier is validated or not.
    /// </summary>
    public bool ValidateClientId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is throttled after a certain limit or not.
    /// </summary>
    public bool ThrottleUser { get; set; }

    /// <summary>
    /// Gets or sets a user's monthly limit in byte.
    /// </summary>
    public long? MonthlyByteLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is a sync user or not.
    /// </summary>
    public bool IsSyncUser { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created at timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the deleted at timestamp.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the updated at timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; } = null;

    /// <summary>
    /// Returns a <see cref="string"></see> representation of the <see cref="MqttUser" /> class.
    /// </summary>
    /// <returns>A <see cref="string"></see> representation of the <see cref="MqttUser" /> class.</returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
