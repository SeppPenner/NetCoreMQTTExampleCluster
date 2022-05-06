// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttClientGrain.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The grain interface for one client identifier.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains.Interfaces;

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
    Task<bool> ProceedSubscription(SimpleMqttSubscriptionInterceptorContext context);

    /// <summary>
    /// Proceeds the published message for one client identifier.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A value indicating whether the published message is accepted or not.</returns>
    Task<bool> ProceedPublish(SimpleMqttApplicationMessageInterceptorContext context);

    /// <summary>
    /// Proceeds the connection for one client identifier.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A value indicating whether the connection is accepted or not.</returns>
    Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context);

    /// <summary>
    /// Checks whether the user is a user used for synchronization.
    /// </summary>
    /// <returns>A value indicating whether the user is a broker user or not.</returns>
    Task<bool> IsUserBrokerUser();

    /// <summary>
    /// Tells the grain to refresh its cache.
    /// </summary>
    /// <param name="force">Forces a cache update.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    Task RefreshCache(bool force);
}
