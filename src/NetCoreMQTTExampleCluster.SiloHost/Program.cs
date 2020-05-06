// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Haemmer Electronics">
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
    using System.Runtime.InteropServices;

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
            var settingsFile = Path.Combine(currentLocation, "NetCoreMQTTExampleCluster.SiloHost.dev.json");
#else
            var settingsFile = Path.Combine(currentLocation, "NetCoreMQTTExampleCluster.SiloHost.json");
#endif
            var settingsString = File.ReadAllText(settingsFile);
            var parsedSettings = JObject.Parse(settingsString);
            var logFolderPath = parsedSettings["LogFolderPath"].ToString();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console(outputTemplate: "{Timestamp: HH:mm:ss} {Level:u3} [{Grain}] [{Id}] {Message}{NewLine}{Exception}"))
                .WriteTo.Async(a => a.File(Path.Combine(logFolderPath, @"NetCoreMQTTExampleCluster.SiloHost_.txt"), outputTemplate: "{Timestamp: HH:mm:ss} {Level:u3} [{Grain}] [{Id}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information($"Current directory: {currentLocation}.");

            var configurationBuilder = new ConfigurationBuilder();

#if DEBUG
            configurationBuilder.AddMqttConfig("NetCoreMQTTExampleCluster.SiloHost.dev.json");
#else
            configurationBuilder.AddMqttConfig("NetCoreMQTTExampleCluster.SiloHost.json");
#endif

            var configuration = configurationBuilder.Build();

            try
            {
                IHost host = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    host = Host
                        .CreateDefaultBuilder()
                        .UseSystemd()
                        .Build();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    host = Host
                        .CreateDefaultBuilder()
                        .UseWindowsService()
                        .Build();
                }

                if (host == null)
                {
                    Log.Fatal("You are using a wrong operating system, only Windows and Linux are supported.");
                    Environment.Exit(-1);
                }

                var lifeTimeService = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var siloHostService = new SiloHostServiceMain(configuration, lifeTimeService);
                lifeTimeService.ApplicationStopped.Register(() => { siloHostService.Dispose(); });
                siloHostService.Start();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
