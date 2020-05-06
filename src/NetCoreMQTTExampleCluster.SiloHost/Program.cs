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

    using Microsoft.Extensions.Configuration;

    using Newtonsoft.Json.Linq;

    using Serilog;

    using Topshelf;

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

            var rc = HostFactory.Run(x =>
                {
                    x.Service<SiloHostServiceMain>(s =>
                        {
                            s.ConstructUsing(name => new SiloHostServiceMain(configuration));
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });

                    x.UseSerilog();

                    x.RunAsLocalSystem();

                    x.SetDescription("Orleans Silo Host for the MQTT broker.");
                    x.SetDisplayName("NetCoreMQTTExampleCluster.SiloHost");
                    x.SetServiceName("NetCoreMQTTExampleCluster.SiloHost");
                });

            Environment.ExitCode = (int)rc;
        }
    }
}
