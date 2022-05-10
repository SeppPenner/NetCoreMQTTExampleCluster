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
    /// The service name.
    /// </summary>
    public static readonly AssemblyName ServiceName = Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// The environment name.
    /// </summary>
    public static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

    /// <summary>
    /// Gets or sets the scada configuration.
    /// </summary>
    public static SiloHostConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// The main method.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public static async Task<int> Main(string[] args)
    {
        ReadConfiguration();
        SetupLogging();

        try
        {
            Log.Information("Starting {ServiceName}, Version {Version}", ServiceName.Name, ServiceName.Version);
            Log.Information("Running on {Ports}", GetPorts());
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            await CreateHostBuilder(args, currentLocation).Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return 0;
    }

    /// <summary>
    /// Gets the ports of the service.
    /// </summary>
    /// <returns>The ports of the service.</returns>
    private static List<int> GetPorts()
    {
        return new List<int>
        {
            Configuration.OrleansConfiguration!.EndpointOptions!.SiloPort,
            Configuration.OrleansConfiguration!.EndpointOptions!.SiloListeningEndpointPort,
            Configuration.OrleansConfiguration!.EndpointOptions!.GatewayPort,
            Configuration.OrleansConfiguration!.EndpointOptions!.GatewayListeningEndpointPort
        };
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="currentLocation">The current assembly location.</param>
    /// <returns>A new <see cref="IHostBuilder"/>.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args, string currentLocation)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder.UseContentRoot(currentLocation);
                    webBuilder.UseStartup<Startup>();
                })
            .UseOrleans(
               (_, builder) =>
               {
                   ConfigureOrleans(builder);
               })
            .UseSerilog()
            .UseWindowsService()
            .UseSystemd();
    }

    /// <summary>
    /// Configures the Orleans silo host startup.
    /// </summary>
    /// <param name="builder">The builder.</param>
    private static void ConfigureOrleans(ISiloBuilder builder)
    {
        var databaseSettings = Configuration.DatabaseSettings!;
        var orleansConfiguration = Configuration.OrleansConfiguration!;
        var clusterOptions = orleansConfiguration.ClusterOptions!;

        builder.ConfigureServices(
            s =>
            {
                s.AddOptions();
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
                options.Invariant = GlobalConstants.Invariant;
                options.ConnectionString = databaseSettings.ToConnectionString();
            });

        builder.UseAdoNetReminderService(
            options =>
            {
                options.Invariant = GlobalConstants.Invariant;
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
                var endpointOptions = orleansConfiguration.EndpointOptions!;
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
                options.Bind(orleansConfiguration.DashboardOptions!);
            });

        builder.AddSimpleMessageStreamProvider(GlobalConstants.SimpleMessageStreamProvider);
        builder.AddMemoryGrainStorage(GlobalConstants.PubSubStore);
    }

    /// <summary>
    /// Reads the configuration.
    /// </summary>
    private static void ReadConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", false, true);

        if (!string.IsNullOrWhiteSpace(EnvironmentName))
        {
            var appsettingsFileName = $"appsettings.{EnvironmentName}.json";

            if (File.Exists(appsettingsFileName))
            {
                configurationBuilder.AddJsonFile(appsettingsFileName, false, true);
            }
        }

        var configuration = configurationBuilder.Build();
        configuration.Bind(ServiceName.Name, Configuration);

        if (!Configuration.IsValid())
        {
            throw new InvalidOperationException("The configuration is invalid!");
        }
    }

    /// <summary>
    /// Setup the logging.
    /// </summary>
    private static void SetupLogging()
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName();

        if (EnvironmentName != "Development")
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(Configuration.LogFolderPath, $@"{ServiceName.Name}_.txt"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true);
        }

        loggerConfiguration.WriteTo.Console();
        Log.Logger = loggerConfiguration.CreateLogger();
    }
}
