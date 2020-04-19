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

    using Serilog;

    using Topshelf;

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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File(Path.Combine(currentLocation, @"log\NetCoreMQTTExampleCluster.Cluster_.txt"), rollingInterval: RollingInterval.Day))
                .CreateLogger();

            Log.Information($"Current directory: {currentLocation}.");

            var rc = HostFactory.Run(
                x =>
                {
                    x.Service<MqttService>(
                        s =>
                        {
                            s.ConstructUsing(name => new MqttService(currentLocation));
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });

                    x.UseSerilog();

                    x.RunAsLocalSystem();

                    x.SetDescription("The NetCoreMQTTExampleCluster cluster.");
                    x.SetDisplayName("NetCoreMQTTExampleCluster.Cluster");
                    x.SetServiceName("NetCoreMQTTExampleCluster.Cluster");
                });

            Environment.ExitCode = (int)rc;
        }
    }
}