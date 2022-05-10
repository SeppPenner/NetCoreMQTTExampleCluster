// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Cluster;

/// <summary>
/// The startup class.
/// </summary>
public class Startup
{
    /// <summary>
    /// The SCADA configuration.
    /// </summary>
    private readonly ClusterConfiguration mqttConfiguration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public Startup(IConfiguration configuration)
    {
        configuration.GetSection(Program.ServiceName.Name).Bind(this.mqttConfiguration);
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Add configuration.
        services.AddOptions();
        services.AddSingleton(this.mqttConfiguration);

        // Add the logger.
        services.AddSingleton(Log.Logger);

        // Add JSON converters.
        services.AddMvc()
            .AddRazorPagesOptions(options => options.RootDirectory = "/")
            .AddDataAnnotationsLocalization();

        // Workaround to have a hosted background service available by DI.
        services.AddSingleton<MqttService>();
        services.AddSingleton<IHostedService>(p => p.GetRequiredService<MqttService>());
    }

    /// <summary>
    /// This method gets called by the runtime.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The web hosting environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSerilogRequestLogging();
        app.UseRouting();

        _ = app.ApplicationServices.GetService<MqttService>();
    }
}

