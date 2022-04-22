// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost;

/// <summary>
/// A class that contains the main program.
/// </summary>
public class Program
{
    /// <summary>
    /// The invariant.
    /// </summary>
    private const string Invariant = "Npgsql";

    /// <summary>
    /// The service name.
    /// </summary>
    private static readonly AssemblyName ServiceName = Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", false, true);

        if (environment is not null)
        {
            configurationBuilder.AddJsonFile($"appsettings.{environment}.json", false, true);
        }

        var configuration = configurationBuilder.Build();

        var siloHostConfiguration = new SiloHostConfiguration();
        configuration.Bind(ServiceName.Name, siloHostConfiguration);
        var logFolderPath = siloHostConfiguration.LogFolderPath;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName();

        if (environment != "Development")
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(logFolderPath, $@"{ServiceName.Name}_.txt"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true);
        }
        else
        {
            loggerConfiguration.WriteTo.Console();
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        try
        {
            Log.Information($"{ServiceName.Name}, Version {ServiceName.Version}");
            Log.Information("Starting MQTT broker silo host instance...");

            var host = Host
                .CreateDefaultBuilder(args)
                .UseOrleans(
                    (context, builder) =>
                    {
                        ConfigureOrleans(builder, siloHostConfiguration);
                    })
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton(siloHostConfiguration);
                        services.AddHostedService<SiloHostService>();
                    })
                .UseSerilog()
                .UseWindowsService()
                .UseSystemd()
                .Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal("An error occurred: {Exception}.", ex);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Configures the Orleans silo host startup.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="siloHostConfiguration">The silo host configuration.</param>
    private static void ConfigureOrleans(ISiloBuilder builder, SiloHostConfiguration siloHostConfiguration)
    {
        var databaseSettings = siloHostConfiguration.DatabaseSettings;
        var orleansConfiguration = siloHostConfiguration.OrleansConfiguration;
        var clusterOptions = orleansConfiguration.ClusterOptions;

        builder.ConfigureServices(
            s =>
            {
                s.AddSingleton<IEventLogRepository>(r => new EventLogRepository(databaseSettings));
                s.AddSingleton<IBlacklistRepository>(u => new BlacklistRepository(databaseSettings));
                s.AddSingleton<IDatabaseVersionRepository>(u => new DatabaseVersionRepository(databaseSettings));
                s.AddSingleton<IPublishMessageRepository>(a => new PublishMessageRepository(databaseSettings));
                s.AddSingleton<IUserRepository>(u => new UserRepository(databaseSettings));
                s.AddSingleton<IWhitelistRepository>(u => new WhitelistRepository(databaseSettings));
                s.AddSingleton<IMqttValidator>(new MqttValidator());
            });

        builder.UseAdoNetClustering(
            options =>
            {
                options.Invariant = Invariant;
                options.ConnectionString = databaseSettings.ToConnectionString();
            });

        builder.UseAdoNetReminderService(
            options =>
            {
                options.Invariant = Invariant;
                options.ConnectionString = databaseSettings.ToConnectionString();
            });

        builder.Configure<ClusterOptions>(
            options =>
            {
                options.ClusterId = clusterOptions.ClusterId;
                options.ServiceId = clusterOptions.ServiceId;
            });

        builder.Configure<EndpointOptions>(
            options =>
            {
                var endpointOptions = orleansConfiguration.EndpointOptions;
                endpointOptions.Bind(options);
            });

        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(MqttRepositoryGrain).Assembly).WithReferences());

        builder.ConfigureLogging(
            logging =>
            {
                logging.AddSerilog();
            });

        builder.UseDashboard(
            options =>
            {
                options.Bind(orleansConfiguration.DashboardOptions);
            });

        builder.AddSimpleMessageStreamProvider("SMSProvider");
        builder.AddMemoryGrainStorage("PubSubStore");
    }
}
