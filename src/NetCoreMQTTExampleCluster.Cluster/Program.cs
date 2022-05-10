// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster;

/// <summary>
/// The main program.
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
    /// Gets or sets the MQTT service configuration.
    /// </summary>
    public static ClusterConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// The main method that starts the service.
    /// </summary>
    /// <param name="args">Some arguments.</param>
    /// <returns>The result code.</returns>
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
        var list = new List<int>
        {
            Configuration.Port
        };

        if (Configuration.UnencryptedPort is not null)
        {
            list.Add(Configuration.UnencryptedPort.Value);
        }

        return list;
    }

    /// <summary>
    /// Creates the host builder.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="currentLocation">The current assembly location.</param>
    /// <returns>A new <see cref="IHostBuilder"/>.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args, string currentLocation)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
            webBuilder =>
            {
                webBuilder.UseContentRoot(currentLocation);
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog()
            .UseWindowsService()
            .UseSystemd();
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
