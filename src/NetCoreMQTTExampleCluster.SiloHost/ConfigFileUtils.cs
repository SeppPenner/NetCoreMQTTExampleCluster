// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFileUtils.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class that contains extensions for the config file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost
{
    using System.IO;
    using System.Reflection;

    using Microsoft.Extensions.Configuration;

    using Serilog;

    /// <summary>
    /// A class that contains extensions for the config file.
    /// </summary>
    public static class ConfigFileUtils
    {
        /// <summary>
        /// Adds the configuration file to the 
        /// </summary>
        /// <param name="configurationBuilder">The configuration.</param>
        /// <param name="configFileName">The config file name.</param>
        /// <returns>An <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddMqttConfig(
            this IConfigurationBuilder configurationBuilder,
            string configFileName = "NetCoreMQTTExampleCluster.SiloHost.json")
        {
            var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFilePath = FindConfigFile(currentLocation, configFileName);

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {configFileName}.");
            }

            Log.Information($"Using config file: {configFilePath}.");
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configurationBuilder.AddJsonFile(configFilePath, false, true);
            return configurationBuilder;
        }

        /// <summary>
        /// Finds the configuration file.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A <see cref="string"/> from the configuration file.</returns>
        private static string FindConfigFile(string directory, string fileName = "NetCoreMQTTExampleCluster.SiloHost.json")
        {
            while (true)
            {
                var filePath = Path.Combine(directory, fileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }

                if (directory == Path.GetPathRoot(directory))
                {
                    return fileName;
                }

                directory = Path.GetFullPath(Path.Combine(directory, ".."));
            }
        }
    }
}
