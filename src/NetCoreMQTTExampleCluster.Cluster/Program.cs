// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
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
                .WriteTo.Async(a => a.File(Path.Combine(logFolderPath, @"NetCoreMQTTExampleCluster.Cluster_.txt"), rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information("Current directory: {currentLocation}.", currentLocation);

            try
            {
                var host = Host
                    .CreateDefaultBuilder()
                    .UseWindowsService()
                    .UseSystemd()
                    .Build();

                var lifeTimeService = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var mqttService = new MqttService(currentLocation, lifeTimeService);
                lifeTimeService.ApplicationStopped.Register(() => { mqttService.Dispose(); });
                 mqttService.Start();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Error("An error occured: {ex}.", ex);
            }
        }
    }
}