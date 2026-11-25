// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishedMessagePayload.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to serialize n published message payload to the JSON binary field in the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data;

/// <summary>
/// A class to serialize n published message payload to the JSON binary field in the database.
/// </summary>
public class PublishedMessagePayload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedMessagePayload"/> class.
    /// This constructor is needed for JSON (de-)serialization.
    /// </summary>
    public PublishedMessagePayload()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedMessagePayload"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public PublishedMessagePayload(string message)
    {
        this.Message = message;
    }

    /// <summary>
    /// Gets or sets the message payload, as text or, if <see cref="IsBase64"/> is set, as Base64.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Message"/> holds the payload bytes as Base64.
    /// The value is only written to the database if it is set.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsBase64 { get; set; }

    /// <summary>
    /// Creates the payload from the bytes of a message. Valid UTF-8 without the NUL character is stored as text, everything
    /// else as Base64, because a JSONB value can't hold invalid UTF-8 or the NUL character and the insert would fail.
    /// </summary>
    /// <param name="payload">The payload bytes.</param>
    /// <returns>The <see cref="PublishedMessagePayload"/>.</returns>
    public static PublishedMessagePayload FromBytes(byte[] payload)
    {
        if (Utf8.IsValid(payload) && !payload.Contains((byte)0))
        {
            return new PublishedMessagePayload(Encoding.UTF8.GetString(payload));
        }

        return new PublishedMessagePayload(Convert.ToBase64String(payload))
        {
            IsBase64 = true
        };
    }

    /// <summary>
    /// Returns a <see cref="string"></see> representation of the <see cref="PublishedMessagePayload" /> class.
    /// </summary>
    /// <returns>A <see cref="string"></see> representation of the <see cref="PublishedMessagePayload" /> class.</returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
