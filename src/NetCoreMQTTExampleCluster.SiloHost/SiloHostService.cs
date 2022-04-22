// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloHostService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the Orleans silo host main service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost;

/// <inheritdoc cref="BackgroundService"/>
/// <summary>
/// A class that contains the Orleans silo host main service.
/// </summary>
/// <seealso cref="BackgroundService"/>
public class SiloHostService : BackgroundService
{
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext<SiloHostService>();

    /// <summary>
    /// The silo host configuration.
    /// </summary>
    private readonly SiloHostConfiguration siloHostConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SiloHostService" /> class.
    /// </summary>
    /// <param name="siloHostConfiguration">The silo host configuration..</param>
    public SiloHostService(SiloHostConfiguration siloHostConfiguration)
    {
        this.siloHostConfiguration = siloHostConfiguration;
    }

    /// <inheritdoc cref="BackgroundService"/>
    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    /// <seealso cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.Information("Starting MQTT silo host service...");
        await base.StartAsync(cancellationToken);
        this.logger.Information("MQTT silo host service started.");
    }

    /// <inheritdoc cref="BackgroundService"/>
    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    /// <seealso cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.Information("Stopping MQTT silo host service...");
        await base.StopAsync(cancellationToken);
        this.logger.Information("MQTT silo host service stopped.");
    }

    /// <inheritdoc cref="BackgroundService"/>
    /// <summary>
    /// This method is called when the <see cref="BackgroundService"/> starts. The implementation should return a task that represents
    /// the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="BackgroundService.StopAsync(CancellationToken)"/> is called.</param>
    /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
    /// <seealso cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.Information("Heartbeat.");
            await Task.Delay(this.siloHostConfiguration.HeartbeatIntervalInMilliseconds, stoppingToken);
        }
    }
}
