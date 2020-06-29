// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttRepositoryGrain.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The grain interface for a repository to manage the brokers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using MQTTnet.Server;

    using Orleans;

    /// <summary>
    /// The grain interface for a repository to manage the brokers.
    /// </summary>
    public interface IMqttRepositoryGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// Connects a broker to the grain.
        /// </summary>
        /// <param name="brokerConnectionSettings">The broker connection settings.</param>
        /// <param name="brokerId">The broker identifier.</param>
        void ConnectBroker(IBrokerConnectionSettings brokerConnectionSettings, Guid brokerId);

        /// <summary>
        /// Disconnects the broker from the grain.
        /// </summary>
        /// <param name="brokerId">The broker identifier.</param>
        void DisconnectBroker(Guid brokerId);

        /// <summary>
        /// Proceeds the subscription.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        Task<bool> ProceedSubscription(MqttSubscriptionInterceptorContext context);

        /// <summary>
        /// Proceeds the published message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brokerId">The broker identifier.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        Task<bool> ProceedPublish(MqttApplicationMessageInterceptorContext context, Guid brokerId);

        /// <summary>
        /// Proceeds the unsubscription for one client identifier.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        Task ProceedUnsubscription(MqttServerClientUnsubscribedTopicEventArgs eventArgs);

        /// <summary>
        /// Proceeds the connection for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context);

        /// <summary>
        /// Proceeds the disconnection for one client identifier.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
        Task ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs);
    }
}
