// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloHostServiceMain.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the Orleans silo host main service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost
{
    using System;
    using System.Threading;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using NetCoreMQTTExampleCluster.Grains;
    using NetCoreMQTTExampleCluster.Storage;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;
    using NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;
    using NetCoreMQTTExampleCluster.Validation;

    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;

    using Serilog;

    using ILogger = Serilog.ILogger;

    /// <inheritdoc cref="IDisposable"/>
    /// <summary>
    ///     A class that contains the Orleans silo host main service.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class SiloHostServiceMain : IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger = Log.ForContext<SiloHostServiceMain>();

        /// <summary>
        ///     The configuration.
        /// </summary>
        private readonly IConfigurationRoot configuration;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        ///     The silo host.
        /// </summary>
        private ISiloHost siloHost;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SiloHostServiceMain" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="applicationLifetime">The application life time.</param>
        public SiloHostServiceMain(IConfigurationRoot configuration, IHostApplicationLifetime applicationLifetime)
        {
            this.configuration = configuration;
            this.cancellationToken = applicationLifetime.ApplicationStopping;
        }

        /// <summary>
        ///     Starts the service.
        /// </summary>
        public void Start()
        {
            try
            {
                this.siloHost = this.StartSilo();
                this.siloHost.StartAsync(this.cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.Error("Start of the silo host failed: {ex}.", ex);
                throw;
            }
        }

        /// <inheritdoc cref="IDisposable"/>
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <seealso cref="IDisposable"/>
        public async void Dispose()
        {
            if (this.siloHost != null)
            {
                await this.siloHost.StopAsync(this.cancellationToken);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Starts the silo host.
        /// </summary>
        /// <returns>An <see cref="ISiloHost" />.</returns>
        private ISiloHost StartSilo()
        {
            var databaseConfig = this.configuration.GetSection("Database");
            var connectionSettings = new MqttDatabaseConnectionSettings();
            databaseConfig.Bind(connectionSettings);

            DashboardOptions dashboardOptions = null;

            if (this.configuration.GetSection("Orleans").GetSection("DashboardOptions").Exists())
            {
                dashboardOptions = new DashboardOptions();
                this.configuration.GetSection("Orleans").Bind("DashboardOptions", dashboardOptions);
            }

            // ReSharper disable ImplicitlyCapturedClosure
            var builder = new SiloHostBuilder().ConfigureServices(
                s =>
                {
                    s.Configure<MqttDatabaseConnectionSettings>(databaseConfig);
                    s.AddSingleton<IEventLogRepository>(r => new EventLogRepository(connectionSettings));
                    s.AddSingleton<IBlacklistRepository>(u => new BlacklistRepository(connectionSettings));
                    s.AddSingleton<IDatabaseVersionRepository>(u => new DatabaseVersionRepository(connectionSettings));
                    s.AddSingleton<IPublishMessageRepository>(a => new PublishMessageRepository(connectionSettings));
                    s.AddSingleton<IUserRepository>(u => new UserRepository(connectionSettings));
                    s.AddSingleton<IWhitelistRepository>(u => new WhitelistRepository(connectionSettings));
                    s.AddSingleton<IMqttValidator>(new MqttValidator());
                }).UseAdoNetClustering(
                options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = connectionSettings.ToConnectionString();
                }).UseAdoNetReminderService(
                options =>
                {
                    options.Invariant = "Npgsql";
                    options.ConnectionString = connectionSettings.ToConnectionString();
                }).Configure<ClusterOptions>(options => { this.configuration.GetSection("Orleans").Bind("ClusterOptions", options); }).Configure<EndpointOptions>(
                options =>
                {
                    var opt = new SiloEndpointOptions();
                    this.configuration.GetSection("Orleans").Bind("EndpointOptions", opt);
                    opt.Bind(options);
                }).ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(MqttRepositoryGrain).Assembly).WithReferences()).ConfigureLogging(
                logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(dispose: true, logger: this.logger);
                });

            if (dashboardOptions != null)
            {
                builder.UseDashboard(
                    o =>
                    {
                        o.HostSelf = true;
                        o.CounterUpdateIntervalMs = dashboardOptions.CounterUpdateIntervalMs;
                        o.Port = dashboardOptions.Port;
                    });
            }

            builder.AddSimpleMessageStreamProvider("SMSProvider").AddMemoryGrainStorage("PubSubStore");

            var host = builder.Build();
            return host;
        }
    }
}
