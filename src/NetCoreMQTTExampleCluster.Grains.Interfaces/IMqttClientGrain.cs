// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttClientGrain.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The grain interface for one client identifier.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces
{
    using System.Threading.Tasks;

    using MQTTnet.Server;

    using Orleans;

    /// <summary>
    /// The grain interface for one client identifier.
    /// </summary>
    public interface IMqttClientGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Proceeds the subscription for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the subscription is accepted or not.</returns>
        Task<bool> ProceedSubscription(MqttSubscriptionInterceptorContext context);

        /// <summary>
        /// Proceeds the published message for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the published message is accepted or not.</returns>
        Task<bool> ProceedPublish(MqttApplicationMessageInterceptorContext context);

        /// <summary>
        /// Proceeds the connection for one client identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A value indicating whether the connection is accepted or not.</returns>
        Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context);

        /// <summary>
        /// Proceeds the disconnection message for one client identifier.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        void ProceedDisconnect(MqttServerClientDisconnectedEventArgs eventArgs);

        /// <summary>
        /// Checks whether the user is a user used for synchronization.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>A value indicating whether the user is a broker user or not.</returns>
        Task<bool> IsUserBrokerUser(string clientId);
    }
}