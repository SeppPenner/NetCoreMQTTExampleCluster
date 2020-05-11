// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Newtonsoft.Json.Linq;

    using Serilog;

    /// <summary>
    ///     The main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     The main method that starts the service using Topshelf.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static void Main()
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if DEBUG
            var settingsFile = Path.Combine(currentLocation, "appsettings.Development.json");
#else
            var settingsFile = Path.Combine(currentLocation, "appsettings.json");
#endif
            var settingsString = File.ReadAllText(settingsFile);
            var parsedSettings = JObject.Parse(settingsString);
            var logFolderPath = parsedSettings["LogFolderPath"].ToString();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File(Path.Combine(logFolderPath, @"NetCoreMQTTExampleCluster.Cluster_.txt"), rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information($"Current directory: {currentLocation}.");

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
                var mqttService = new MqttService(currentLocation, lifeTimeService);
                lifeTimeService.ApplicationStopped.Register(() => { mqttService.Dispose(); });
                mqttService.Start();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}