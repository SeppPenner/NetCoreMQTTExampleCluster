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
    private static readonly IPasswordHasher<User> PasswordHasher = new PasswordHasher<User>();

    /// <summary>
    /// The user repository.
    /// </summary>
    private readonly IUserRepository userRepository;

    /// <summary>
    /// The MQTT validator.
    /// </summary>
    private readonly IMqttValidator mqttValidator;

    /// <summary>
    /// The user.
    /// </summary>
    private User user;

    /// <summary>
    /// The user data.
    /// </summary>
    private UserData userData = new();

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
    public MqttClientGrain(IUserRepository userRepository, IMqttValidator mqttValidator)
    {
        this.userRepository = userRepository;
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
            this.user = await this.userRepository.GetUserByName(context.UserName);

            if (this.user is null)
            {
                return false;
            }

            await this.RefreshCache(false);
            return this.mqttValidator.ValidateConnection(context, this.user, PasswordHasher);
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
            var result = this.mqttValidator.ValidatePublish(context, this.userData.PublishBlacklist, this.userData.PublishWhitelist, this.user, DataLimitCacheMonth, this.userData.ClientIdPrefixes);
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
            var result = this.mqttValidator.ValidateSubscription(context, this.userData.SubscriptionBlacklist, this.userData.SubscriptionWhitelist, this.user, this.userData.ClientIdPrefixes);
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
            var result = this.user.IsSyncUser;
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

        this.userData = await this.userRepository.GetUserData(this.user.Id);
        this.cacheLoaded = true;
    }
}
