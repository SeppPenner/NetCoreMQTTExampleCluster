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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console(outputTemplate: "{Timestamp: HH:mm:ss} {Level:u3} [{Grain}] [{Id}] {Message}{NewLine}{Exception}"))
                .WriteTo.Async(a => a.File(Path.Combine(currentLocation, @"log\NetCoreMQTTExampleCluster.SiloHost_.txt"), outputTemplate: "{Timestamp: HH:mm:ss} {Level:u3} [{Grain}] [{Id}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information($"Current directory: {currentLocation}.");

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddMqttConfig();
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
