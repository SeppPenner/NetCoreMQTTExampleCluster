// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttValidator.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to validate the different MQTT contexts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Validation;

/// <inheritdoc cref="IMqttValidator"/>
public class MqttValidator : IMqttValidator
{
    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly ILogger Logger = Log.ForContext<MqttValidator>();

    /// <inheritdoc cref="IMqttValidator"/>
    public bool ValidateConnection(SimpleMqttConnectionValidatorContext context, User user, IPasswordHasher<User> passwordHasher)
    {
        Logger.Debug("Executed ValidateConnection with parameters: {@context}, {@user}.", context, user);

        Logger.Debug("Current user is {@currentUser}.", user);

        if (user is null)
        {
            Logger.Debug("Current user was null.");
            return false;
        }

        if (context.UserName != user.UserName)
        {
            Logger.Debug("User name in context doesn't match the current user.");
            return false;
        }

        var hashingResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, context.Password);

        if (hashingResult == PasswordVerificationResult.Failed)
        {
            Logger.Debug("Password verification failed.");
            return false;
        }

        if (!user.ValidateClientId)
        {
            Logger.Debug("Connection valid for {@clientId} and {@user}.", context.ClientId, user);
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.ClientIdPrefix))
        {
            if (context.ClientId != user.ClientId)
            {
                Logger.Debug("Client id in context doesn't match the current user's client id.");
                return false;
            }

            Logger.Debug("Connection valid for {@clientId} and {@user} when client id prefix was null.", context.ClientId, user);
        }
        else
        {
            Logger.Debug("Connection valid for {@clientIdPrefix} and {@user} when client id prefix was not null.", user.ClientIdPrefix, user);
        }

        return true;
    }

    /// <inheritdoc cref="IMqttValidator"/>
    public bool ValidatePublish(
        MqttApplicationMessageInterceptorContext context,
        List<BlacklistWhitelist> blacklist,
        List<BlacklistWhitelist> whitelist,
        User user,
        IMemoryCache dataLimitCacheMonth,
        List<string> clientIdPrefixes)
    {
        Logger.Debug("Executed ValidatePublish with parameters: {@context}, {@user}.", context, user);

        var clientIdPrefix = GetClientIdPrefix(context.ClientId, clientIdPrefixes);

        Logger.Debug("Client id prefix is {@clientIdPrefix}.", clientIdPrefix);

        if (user is null)
        {
            Logger.Debug("Current user was null.");
            return false;
        }

        var topic = context.ApplicationMessage.Topic;

        Logger.Debug("Topic was {@topic}.", topic);

        if (user.ThrottleUser)
        {
            var payload = context.ApplicationMessage?.Payload;

            if (payload is not null)
            {
                if (user.MonthlyByteLimit is not null)
                {
                    if (IsUserThrottled(
                        context.ClientId,
                        payload.Length,
                        user.MonthlyByteLimit.Value,
                        dataLimitCacheMonth))
                    {
                        Logger.Debug("User is throttled now.");
                        return false;
                    }
                }
            }
        }

        Logger.Debug("The blacklist was {@blacklist}.", blacklist);
        Logger.Debug("The whitelist was {@whitelist}.", whitelist);

        // Check matches
        if (blacklist.Any(b => b.Value == topic))
        {
            Logger.Debug("The blacklist matched a topic.");
            return false;
        }

        if (whitelist.Any(b => b.Value == topic))
        {
            Logger.Debug("The whitelist matched a topic.");
            return true;
        }

        foreach (var forbiddenTopic in blacklist)
        {
            var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

            if (!doesTopicMatch)
            {
                continue;
            }

            Logger.Debug("The blacklist matched a topic with regex.");
            return false;
        }

        foreach (var allowedTopic in whitelist)
        {
            var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

            if (!doesTopicMatch)
            {
                continue;
            }

            Logger.Debug("The whitelist matched a topic with regex.");
            return true;
        }

        Logger.Warning("We fell through everything else. This should never happen! Context was {@context}.", context);
        return false;
    }

    /// <inheritdoc cref="IMqttValidator"/>
    public bool ValidateSubscription(
        MqttSubscriptionInterceptorContext context,
        List<BlacklistWhitelist> blacklist,
        List<BlacklistWhitelist> whitelist,
        User user,
        List<string> clientIdPrefixes)
    {
        Logger.Debug("Executed ValidateSubscription with parameters: {@context}, {@user}.", context, user);

        var clientIdPrefix = GetClientIdPrefix(context.ClientId, clientIdPrefixes);

        Logger.Debug("Client id prefix is {@clientIdPrefix}.", clientIdPrefix);

        if (user is null)
        {
            Logger.Debug("Current user was null.");
            return false;
        }

        var topic = context.TopicFilter.Topic;

        Logger.Debug("Topic was {@topic}.", topic);
        Logger.Debug("The blacklist was {@blacklist}.", blacklist);
        Logger.Debug("The whitelist was {@whitelist}.", whitelist);

        // Check matches
        if (blacklist.Any(b => b.Value == topic))
        {
            Logger.Debug("The blacklist matched a topic.");
            return false;
        }

        if (whitelist.Any(b => b.Value == topic))
        {
            Logger.Debug("The whitelist matched a topic.");
            return true;
        }

        foreach (var forbiddenTopic in blacklist)
        {
            var doesTopicMatch = TopicChecker.Regex(forbiddenTopic.Value, topic);

            if (!doesTopicMatch)
            {
                continue;
            }

            Logger.Debug("The blacklist matched a topic with regex.");
            return false;
        }

        foreach (var allowedTopic in whitelist)
        {
            var doesTopicMatch = TopicChecker.Regex(allowedTopic.Value, topic);

            if (!doesTopicMatch)
            {
                continue;
            }

            Logger.Debug("The whitelist matched a topic with regex.");
            return true;
        }

        Logger.Warning(
            "We fell through everything else. This should never happen! Context was {@context}.",
            context);
        return false;
    }

    /// <summary>
    /// Gets the client identifier prefix for a client identifier if there is one or <c>null</c> else.
    /// </summary>
    /// <param name="clientIdentifierParam">The client identifier.</param>
    /// <param name="clientIdPrefixes">The client identifier prefixes.</param>
    /// <returns>The client identifier prefix for a client identifier if there is one or <c>null</c> else.</returns>
    private static string? GetClientIdPrefix(string clientIdentifierParam, List<string> clientIdPrefixes)
    {
        Logger.Debug("The client id parameter was {@clientIdentifierParam}.", clientIdentifierParam);
        Logger.Debug("The client id prefixes were {@clientIdPrefixes}.", clientIdPrefixes);

        var firstOrDefaultClientIdPrefix = clientIdPrefixes.FirstOrDefault(clientIdentifierParam.StartsWith);
        Logger.Debug("The first or default client id prefix was {@firstOrDefaultClientIdPrefix}.", firstOrDefaultClientIdPrefix);
        return firstOrDefaultClientIdPrefix;
    }

    /// <summary>
    /// Checks whether a user has used the maximum of its publishing limit for the month or not.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="sizeInBytes">The message size in bytes.</param>
    /// <param name="monthlyByteLimit">The monthly byte limit.</param>
    /// <param name="dataLimitCacheMonth">The data limit cache for the month.</param>
    /// <returns>A value indicating whether the user will be throttled or not.</returns>
    private static bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit, IMemoryCache dataLimitCacheMonth)
    {
        dataLimitCacheMonth.TryGetValue(clientId, out var foundByteLimit);

        if (foundByteLimit is null)
        {
            dataLimitCacheMonth.Set(clientId, sizeInBytes, DateTimeOffset.Now.EndOfMonth());

            if (sizeInBytes < monthlyByteLimit)
            {
                return false;
            }

            Logger.Information("The client with client identifier {@clientId} is now locked until the end of this month because it already used its data limit.", clientId);
            return true;
        }

        try
        {
            var currentValue = Convert.ToInt64(foundByteLimit);
            currentValue = checked(currentValue + sizeInBytes);
            dataLimitCacheMonth.Remove(clientId);
            dataLimitCacheMonth.Set(clientId, currentValue, DateTimeOffset.Now.EndOfMonth());

            if (currentValue >= monthlyByteLimit)
            {
                Logger.Information("The client with client identifier {@clientId} is now locked until the end of this month because it already used its data limit.", clientId);
                return true;
            }
        }
        catch (OverflowException)
        {
            Logger.Warning("OverflowException thrown.");
            Logger.Information("The client with client identifier {@clientId} is now locked until the end of this month because it already used its data limit.", clientId);
            return true;
        }

        return false;
    }
}
