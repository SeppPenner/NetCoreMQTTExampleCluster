// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttClientGrain.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The grain for one client identifier.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains;

/// <inheritdoc cref="IMqttClientGrain" />
public class MqttClientGrain : Grain, IMqttClientGrain
{
    /// <summary>
    /// Gets or sets the data limit cache for throttling for monthly data.
    /// </summary>
    private static readonly MemoryCache DataLimitCacheMonth = new(new MemoryCacheOptions());

    /// <summary>
    /// The <see cref="IPasswordHasher{TUser}" />.
    /// </summary>
    private static readonly IPasswordHasher<MqttUser> PasswordHasher = new PasswordHasher<MqttUser>();

    /// <summary>
    /// The MQTT user repository.
    /// </summary>
    private readonly IMqttUserRepository mqttUserRepository;

    /// <summary>
    /// The MQTT validator.
    /// </summary>
    private readonly IMqttValidator mqttValidator;

    /// <summary>
    /// The MQTT user.
    /// </summary>
    private MqttUser? mqttUser;

    /// <summary>
    /// The MQTT user data.
    /// </summary>
    private MqttUserData userData = new();

    /// <summary>
    /// A value indicating whether the caches are loaded or not.
    /// </summary>
    private bool cacheLoaded;

    /// <summary>
    /// The client identifier.
    /// </summary>
    private string clientId = string.Empty;

    /// <summary>
    /// The logger.
    /// </summary>
    private ILogger logger;

    /// <inheritdoc cref="IMqttClientGrain" />
    public MqttClientGrain(IMqttUserRepository mqttUserRepository, IMqttValidator mqttValidator)
    {
        this.mqttUserRepository = mqttUserRepository;
        this.mqttValidator = mqttValidator;
        this.logger = Log.ForContext("Grain", nameof(MqttClientGrain));
    }

    /// <inheritdoc cref="Grain" />
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.clientId = this.GetPrimaryKeyString();
        this.logger = Log.ForContext("Grain", nameof(MqttClientGrain)).ForContext("Id", this.clientId);
        return base.OnActivateAsync(cancellationToken);
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public async Task<bool> ProceedConnect(SimpleValidatingConnectionEventArgs context)
    {
        try
        {
            var connectingUser = await this.mqttUserRepository.GetMqttUserByName(context.UserName);

            // A refused connection must not replace the user of a connected client with the same client identifier.
            if (connectingUser is null || !this.mqttValidator.ValidateConnection(context, connectingUser, PasswordHasher))
            {
                return false;
            }

            // Another user may connect with the same client identifier, then its lists replace the cached ones.
            var userChanged = this.mqttUser?.Id != connectingUser.Id;
            this.mqttUser = connectingUser;
            await this.RefreshCache(userChanged);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return false;
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public async Task<bool> ProceedPublish(SimpleInterceptingPublishEventArgs context)
    {
        try
        {
            if (!await this.EnsureUserLoaded(context.UserName))
            {
                return false;
            }

            return this.mqttValidator.ValidatePublish(context, this.userData.PublishBlacklist, this.userData.PublishWhitelist, this.mqttUser!, DataLimitCacheMonth, this.userData.ClientIdPrefixes);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return false;
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public async Task<bool> ProceedSubscription(SimpleInterceptingSubscriptionEventArgs context)
    {
        try
        {
            if (!await this.EnsureUserLoaded(context.UserName))
            {
                return false;
            }

            return this.mqttValidator.ValidateSubscription(context, this.userData.SubscriptionBlacklist, this.userData.SubscriptionWhitelist, this.mqttUser!, this.userData.ClientIdPrefixes);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return false;
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public Task<bool> IsUserBrokerUser()
    {
        try
        {
            var result = this.mqttUser!.IsSyncUser;
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public async Task RefreshCache(bool force)
    {
        if (!force)
        {
            if (this.cacheLoaded)
            {
                return;
            }
        }

        this.userData = await this.mqttUserRepository.GetMqttUserData(this.mqttUser!.Id);
        this.cacheLoaded = true;
    }

    /// <summary>
    /// Loads the MQTT user if the grain doesn't know it. This happens if the client connected before the grain was deactivated
    /// (e.g. after 15 minutes without calls) or before the silo was restarted. The broker already validated the connection then.
    /// </summary>
    /// <param name="userName">The user name of the connection.</param>
    /// <returns>A value indicating whether the user is loaded or not.</returns>
    private async Task<bool> EnsureUserLoaded(string userName)
    {
        if (this.mqttUser is not null)
        {
            return true;
        }

        this.mqttUser = await this.mqttUserRepository.GetMqttUserByName(userName);

        if (this.mqttUser is null)
        {
            this.logger.Warning("The MQTT user {UserName} of a connected client was not found.", userName);
            return false;
        }

        await this.RefreshCache(false);
        return true;
    }
}
