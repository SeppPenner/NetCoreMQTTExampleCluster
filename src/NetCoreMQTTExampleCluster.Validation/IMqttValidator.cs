// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMqttValidator.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A interface to validate the different MQTT contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation;

/// <summary>
/// A interface to validate the different MQTT contexts.
/// </summary>
public interface IMqttValidator
{
    /// <summary>
    /// Validates the connection.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="mqttUser">The MQTT user.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <returns>A value indicating whether the connection is accepted or not.</returns>
    bool ValidateConnection(
        SimpleMqttConnectionValidatorContext context,
        MqttUser mqttUser,
        IPasswordHasher<MqttUser> passwordHasher);

    /// <summary>
    /// Validates the message publication.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="blacklist">The blacklist.</param>
    /// <param name="whitelist">The whitelist.</param>
    /// <param name="mqttUser">The MQTT user.</param>
    /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
    /// <param name="clientIdPrefixes">The client identifier prefixes.</param>
    /// <returns>A value indicating whether the published message is accepted or not.</returns>
    bool ValidatePublish(
        SimpleMqttApplicationMessageInterceptorContext context,
        List<BlacklistWhitelist> blacklist,
        List<BlacklistWhitelist> whitelist,
        MqttUser mqttUser,
        IMemoryCache dataLimitCacheMonth,
        List<string> clientIdPrefixes);

    /// <summary>
    /// Validates the subscription.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="blacklist">The blacklist.</param>
    /// <param name="whitelist">The whitelist.</param>
    /// <param name="mqttUser">The MQTT user.</param>
    /// <param name="clientIdPrefixes">The client identifier prefixes.</param>
    /// <returns>A value indicating whether the subscription is accepted or not.</returns>
    bool ValidateSubscription(
        SimpleMqttSubscriptionInterceptorContext context,
        List<BlacklistWhitelist> blacklist,
        List<BlacklistWhitelist> whitelist,
        MqttUser mqttUser,
        List<string> clientIdPrefixes);
}
