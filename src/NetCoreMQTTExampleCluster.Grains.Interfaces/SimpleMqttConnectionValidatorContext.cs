// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleMqttConnectionValidatorContext.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains a simplified version of the <see cref="MqttConnectionValidatorContext" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces
{
    using MQTTnet.Server;

    /// <summary>
    /// A class that contains a simplified version of the <see cref="MqttConnectionValidatorContext" />.
    /// </summary>
    public class SimpleMqttConnectionValidatorContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMqttConnectionValidatorContext"/> class. This is used for testing purposes only!
        /// </summary>
        public SimpleMqttConnectionValidatorContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMqttConnectionValidatorContext"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SimpleMqttConnectionValidatorContext(MqttConnectionValidatorContext context)
        {
            this.UserName = context.Username;
            this.ClientId = context.ClientId;
            this.Password = context.Password;
            this.CleanSession = context.CleanSession;
            this.Endpoint = context.Endpoint;
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a clean session is used or not.
        /// </summary>
        public bool? CleanSession { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        public string Endpoint { get; set; }
    }
}
