// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishedMessagePayload.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class to serialize n published message payload to the JSON binary field in the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Data
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class to serialize n published message payload to the JSON binary field in the database.
    /// </summary>
    public class PublishedMessagePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedMessagePayload"/> class.
        /// This constructor is needed for JSON (de-)serialization.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
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
        /// Gets or sets the message payload.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Returns a <see cref="string"></see> representation of the <see cref="PublishedMessagePayload" /> class.
        /// </summary>
        /// <returns>A <see cref="string"></see> representation of the <see cref="PublishedMessagePayload" /> class.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
