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
    /// The MQTT service configuration.
    /// </summary>
    private readonly ClusterConfiguration mqttConfiguration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public Startup(IConfiguration configuration)
    {
        configuration.GetSection(Program.ServiceName.Name ?? "NetCoreMQTTExampleCluster.Cluster").Bind(this.mqttConfiguration);
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

        // Add razor pages.
        services.AddConnections();
        services.AddRazorPages();

        // Add server side Blazor stuff.
        services.AddServerSideBlazor();
        services.AddSignalR();

        // Add response compression.
        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[]
                {
                        MediaTypeNames.Application.Octet
                });
        });

        // Add authentication.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "MqttClusterIssuer",
                ValidAudience = "MqttClusterAudience",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.mqttConfiguration.JsonWebTokenConfigurationKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add authorization.
        services.AddAuthorizationCore();

        // Workaround to have a hosted background service available by DI.
        services.AddSingleton<MqttService>();
        services.AddSingleton<IHostedService>(p => p.GetRequiredService<MqttService>());

        // Add localization.
        services.AddLocalization(options => options.ResourcesPath = "Resources");
    }

    /// <summary>
    /// This method gets called by the runtime.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The web hosting environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Show development errors in development only.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Use response compression.
        app.UseResponseCompression();

        // Use Blazor static files.
        app.UseStaticFiles();

        // Use routing and request logging.
        app.UseSerilogRequestLogging();
        app.UseRouting();

        // Use authentication and authorization.
        app.UseAuthentication();
        app.UseAuthorization();

        _ = app.ApplicationServices.GetService<MqttService>();
    }
}

