// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The web user class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data;

/// <summary>
/// The web user class.
/// </summary>
public class WebUser
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
