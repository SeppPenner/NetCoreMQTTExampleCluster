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
    public override Task OnActivateAsync()
    {
        this.clientId = this.GetPrimaryKeyString();
        this.logger = Log.ForContext("Grain", nameof(MqttClientGrain)).ForContext("Id", this.clientId);
        return base.OnActivateAsync();
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public async Task<bool> ProceedConnect(SimpleMqttConnectionValidatorContext context)
    {
        try
        {
            this.mqttUser = await this.mqttUserRepository.GetUserByName(context.UserName);

            if (this.mqttUser is null)
            {
                return false;
            }

            await this.RefreshCache(false);
            return this.mqttValidator.ValidateConnection(context, this.mqttUser, PasswordHasher);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return false;
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public Task<bool> ProceedPublish(SimpleMqttApplicationMessageInterceptorContext context)
    {
        try
        {
            var result = this.mqttValidator.ValidatePublish(context, this.userData.PublishBlacklist, this.userData.PublishWhitelist, this.mqttUser!, DataLimitCacheMonth, this.userData.ClientIdPrefixes);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc cref="IMqttClientGrain" />
    public Task<bool> ProceedSubscription(SimpleMqttSubscriptionInterceptorContext context)
    {
        try
        {
            var result = this.mqttValidator.ValidateSubscription(context, this.userData.SubscriptionBlacklist, this.userData.SubscriptionWhitelist, this.mqttUser!, this.userData.ClientIdPrefixes);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromResult(false);
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

        this.userData = await this.mqttUserRepository.GetUserData(this.mqttUser!.Id);
        this.cacheLoaded = true;
    }
}
