// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains the main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost
{
    using System;
    using System.IO;
    using System.Reflection;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Newtonsoft.Json.Linq;

    using Serilog;

    /// <summary>
    /// A class that contains the main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method.
        /// </summary>
        public static void Main()
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if DEBUG
            // ReSharper disable once AssignNullToNotNullAttribute
            var settingsFile = Path.Combine(currentLocation, "appsettings.Development.json");
#else
            var settingsFile = Path.Combine(currentLocation, "appsettings.json");
#endif
            var settingsString = File.ReadAllText(settingsFile);
            var parsedSettings = JObject.Parse(settingsString);
            // ReSharper disable once PossibleNullReferenceException
            var logFolderPath = parsedSettings["LogFolderPath"].ToString();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File(Path.Combine(logFolderPath, @"NetCoreMQTTExampleCluster.SiloHost_.txt"), rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information("Current directory: {currentLocation}.", currentLocation);

            var configurationBuilder = new ConfigurationBuilder();

#if DEBUG
            configurationBuilder.AddMqttConfig("appsettings.Development.json");
#else
            configurationBuilder.AddMqttConfig("appsettings.json");
#endif

            var configuration = configurationBuilder.Build();

            try
            {
                var host = Host
                    .CreateDefaultBuilder()
                    .UseWindowsService()
                    .UseSystemd()
                    .Build();

                var lifeTimeService = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var siloHostService = new SiloHostServiceMain(configuration, lifeTimeService);
                lifeTimeService.ApplicationStopped.Register(() => { siloHostService.Dispose(); });
                siloHostService.Start();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Error("An error occured: {ex}.", ex);
            }
        }
    }
}
