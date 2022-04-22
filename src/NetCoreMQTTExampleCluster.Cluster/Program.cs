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
    private static readonly AssemblyName ServiceName = Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// The main method that starts the service using Topshelf.
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

        var clusterConfiguration = new ClusterConfiguration();
        configuration.Bind(ServiceName.Name, clusterConfiguration);
        var logFolderPath = clusterConfiguration.LogFolderPath;

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
            Log.Information("Starting MQTT broker cluster instance...");

            var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(clusterConfiguration);
                    services.AddHostedService<MqttService>();
                })
                .UseSerilog()
                .UseWindowsService()
                .UseSystemd()
                .Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal("An error occurred: {@ex}.", ex);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
